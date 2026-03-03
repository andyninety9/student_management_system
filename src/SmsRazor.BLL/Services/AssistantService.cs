using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
#pragma warning disable OPENAI001
using OpenAI.Assistants;

namespace SmsRazor.BLL.Services;

public class AssistantService : IAssistantService
{
    private readonly IConfiguration _configuration;
    private readonly AzureOpenAIClient _openAIClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, string> _connectionThreads = new();
    private string? _assistantId;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public AssistantService(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        var endpoint = _configuration["AIFoundry:ProjectEndpoint"];
        var apiKey = _configuration["AIFoundry:ApiKey"];

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("AIFoundry configuration is missing required settings.");
        }

        var credential = new System.ClientModel.ApiKeyCredential(apiKey);
        _openAIClient = new AzureOpenAIClient(new Uri(endpoint), credential);
    }

    public async IAsyncEnumerable<string> GetStreamingChatMessageAsync(string message, string connectionId, string? studentCode, string modelName, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var assistantClient = _openAIClient.GetAssistantClient();
        
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (string.IsNullOrEmpty(_assistantId))
            {
                var promptPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Prompts", "AssistantPrompt.txt");
                var systemPrompt = System.IO.File.Exists(promptPath) 
                    ? await System.IO.File.ReadAllTextAsync(promptPath, cancellationToken)
                    : "You are a helpful AI Assistant for students. Please provide concise, accurate, and encouraging answers.";
                
                var options = new AssistantCreationOptions {
                    Name = "Student Helper",
                    Instructions = systemPrompt
                };
                
                // Define the get_student_timetable tool using BinaryData for parameters
                options.Tools.Add(new FunctionToolDefinition() 
                {
                    FunctionName = "get_student_timetable",
                    Description = "Lấy thời khóa biểu của sinh viên hiện tại trong học kỳ active.",
                    // Simple empty object schema for no arguments since we use StudentCode from Context
                    Parameters = BinaryData.FromString("{\"type\": \"object\", \"properties\": {}}")
                });

                // Define the get_student_tuition tool
                options.Tools.Add(new FunctionToolDefinition() 
                {
                    FunctionName = "get_student_tuition",
                    Description = "Lấy thông tin học phí của sinh viên hiện tại trong học kỳ active, bao gồm tổng học phí, đã giảm trừ, tổng phải nộp, đã nộp và còn nợ.",
                    Parameters = BinaryData.FromString("{\"type\": \"object\", \"properties\": {}}")
                });

                // Define the get_remaining_syllabus_courses tool
                options.Tools.Add(new FunctionToolDefinition() 
                {
                    FunctionName = "get_remaining_syllabus_courses",
                    Description = "Lấy danh sách các môn học trong syllabus của sinh viên hiện tại, kèm thông tin môn nào có lớp mở trong học kỳ này.",
                    Parameters = BinaryData.FromString("{\"type\": \"object\", \"properties\": {}}")
                });

                // Define the get_available_classes tool
                options.Tools.Add(new FunctionToolDefinition() 
                {
                    FunctionName = "get_available_classes",
                    Description = "Lấy danh sách các lớp học phần đang mở cho một môn học cụ thể trong học kỳ active.",
                    Parameters = BinaryData.FromString("{\"type\": \"object\", \"properties\": {\"courseId\": {\"type\": \"string\", \"format\": \"uuid\", \"description\": \"ID của môn học (CourseId).\"}}, \"required\": [\"courseId\"]}")
                });

                // Define the register_course tool
                options.Tools.Add(new FunctionToolDefinition() 
                {
                    FunctionName = "register_course",
                    Description = "Đăng ký vào một lớp học phần cụ thể cho sinh viên hiện tại.",
                    Parameters = BinaryData.FromString("{\"type\": \"object\", \"properties\": {\"sectionId\": {\"type\": \"string\", \"format\": \"uuid\", \"description\": \"ID của lớp học phần (SectionId) mà sinh viên muốn đăng ký.\"}}, \"required\": [\"sectionId\"]}")
                });
                
                var assistantResult = await assistantClient.CreateAssistantAsync(modelName, options, cancellationToken);
                _assistantId = assistantResult.Value.Id;
            }
        }
        finally
        {
            _semaphore.Release();
        }

        string? threadId;
        if (!_connectionThreads.TryGetValue(connectionId, out threadId))
        {
            var threadResult = await assistantClient.CreateThreadAsync(cancellationToken: cancellationToken);
            threadId = threadResult.Value.Id;
            _connectionThreads[connectionId] = threadId;
        }
        
        await assistantClient.CreateMessageAsync(threadId, MessageRole.User, new [] { MessageContent.FromText(message) }, cancellationToken: cancellationToken);
        
        var streamingResponse = assistantClient.CreateRunStreamingAsync(threadId, _assistantId, cancellationToken: cancellationToken);
        
        await foreach (var update in streamingResponse)
        {
            if (update is MessageContentUpdate contentUpdate)
            {
                if (!string.IsNullOrEmpty(contentUpdate.Text))
                {
                    yield return contentUpdate.Text;
                }
            }
            else if (update is RunUpdate runUpdate && runUpdate.Value.Status == RunStatus.RequiresAction)
            {
                var toolOutputs = new List<ToolOutput>();
                
                foreach (var toolCall in runUpdate.Value.RequiredActions)
                {
                    if (toolCall.FunctionName == "get_student_timetable")
                    {
                        if (string.IsNullOrEmpty(studentCode))
                        {
                            toolOutputs.Add(new ToolOutput(toolCall.ToolCallId, "Lỗi: Không tìm thấy mã sinh viên (StudentCode) trong phiên đăng nhập. Vui lòng đăng nhập lại."));
                        }
                        else
                        {
                            try
                            {
                                // Resolve a scoped service from the provider since Hubs are transient/singleton and DbContexts are scoped
                                using var scope = _serviceProvider.CreateScope();
                                var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
                                var termService = scope.ServiceProvider.GetRequiredService<ITermService>();
                                
                                // Simplified approach: find active term (or get all timetables if possible)
                                var terms = await termService.GetAllTermsAsync();
                                var currentTermId = terms.FirstOrDefault()?.TermId ?? Guid.Empty;
                                
                                var timetables = await enrollmentService.GetStudentTimetableAsync(studentCode, currentTermId);
                                
                                var jsonResult = System.Text.Json.JsonSerializer.Serialize(timetables);
                                toolOutputs.Add(new ToolOutput(toolCall.ToolCallId, jsonResult));
                            }
                            catch (Exception ex)
                            {
                                toolOutputs.Add(new ToolOutput(toolCall.ToolCallId, $"Lỗi khi tải thời khóa biểu: {ex.Message}"));
                            }
                        }
                    }
                    else if (toolCall.FunctionName == "get_student_tuition")
                    {
                        if (string.IsNullOrEmpty(studentCode))
                        {
                            toolOutputs.Add(new ToolOutput(toolCall.ToolCallId, "Lỗi: Không tìm thấy mã sinh viên (StudentCode) trong phiên đăng nhập. Vui lòng đăng nhập lại."));
                        }
                        else
                        {
                            try
                            {
                                using var scope = _serviceProvider.CreateScope();
                                var tuitionService = scope.ServiceProvider.GetRequiredService<ITuitionService>();
                                var termService = scope.ServiceProvider.GetRequiredService<ITermService>();
                                
                                var terms = await termService.GetAllTermsAsync();
                                var currentTermId = terms.FirstOrDefault()?.TermId ?? Guid.Empty;
                                
                                var tuitionSummary = await tuitionService.GetTuitionSummaryAsync(studentCode, currentTermId);
                                
                                var jsonResult = System.Text.Json.JsonSerializer.Serialize(tuitionSummary);
                                toolOutputs.Add(new ToolOutput(toolCall.ToolCallId, jsonResult));
                            }
                            catch (Exception ex)
                            {
                                toolOutputs.Add(new ToolOutput(toolCall.ToolCallId, $"Lỗi khi tải học phí: {ex.Message}"));
                            }
                        }
                    }
                    else if (toolCall.FunctionName == "get_remaining_syllabus_courses")
                    {
                        if (string.IsNullOrEmpty(studentCode))
                        {
                            toolOutputs.Add(new ToolOutput(toolCall.ToolCallId, "Lỗi: Không tìm thấy mã sinh viên (StudentCode) trong phiên đăng nhập. Vui lòng đăng nhập lại."));
                        }
                        else
                        {
                            try
                            {
                                using var scope = _serviceProvider.CreateScope();
                                var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
                                var termService = scope.ServiceProvider.GetRequiredService<ITermService>();
                                
                                var terms = await termService.GetAllTermsAsync();
                                var currentTermId = terms.FirstOrDefault()?.TermId ?? Guid.Empty;
                                
                                var syllabusCourses = await enrollmentService.GetStudentSyllabusCoursesAsync(studentCode, currentTermId);
                                
                                // Optionally filter out courses already passed if needed, but for now we return what the service provides.
                                var jsonResult = System.Text.Json.JsonSerializer.Serialize(syllabusCourses);
                                toolOutputs.Add(new ToolOutput(toolCall.ToolCallId, jsonResult));
                            }
                            catch (Exception ex)
                            {
                                toolOutputs.Add(new ToolOutput(toolCall.ToolCallId, $"Lỗi khi tải syllabus: {ex.Message}"));
                            }
                        }
                    }
                    else if (toolCall.FunctionName == "get_available_classes")
                    {
                        try
                        {
                            var args = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(toolCall.FunctionArguments);
                            if (args.TryGetProperty("courseId", out var courseIdProp) && Guid.TryParse(courseIdProp.GetString(), out Guid courseId))
                            {
                                using var scope = _serviceProvider.CreateScope();
                                var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
                                var termService = scope.ServiceProvider.GetRequiredService<ITermService>();
                                
                                var terms = await termService.GetAllTermsAsync();
                                var currentTermId = terms.FirstOrDefault()?.TermId ?? Guid.Empty;

                                var availableSections = await enrollmentService.GetAvailableSectionsForCourseAsync(courseId, currentTermId);
                                
                                var jsonResult = System.Text.Json.JsonSerializer.Serialize(availableSections);
                                toolOutputs.Add(new ToolOutput(toolCall.ToolCallId, jsonResult));
                            }
                            else
                            {
                                toolOutputs.Add(new ToolOutput(toolCall.ToolCallId, "Lỗi: Tham số courseId không hợp lệ."));
                            }
                        }
                        catch (Exception ex)
                        {
                            toolOutputs.Add(new ToolOutput(toolCall.ToolCallId, $"Lỗi khi tải lớp học phần: {ex.Message}"));
                        }
                    }
                    else if (toolCall.FunctionName == "register_course")
                    {
                        if (string.IsNullOrEmpty(studentCode))
                        {
                            toolOutputs.Add(new ToolOutput(toolCall.ToolCallId, "Lỗi: Không tìm thấy mã sinh viên (StudentCode) trong phiên đăng nhập. Vui lòng đăng nhập lại."));
                        }
                        else
                        {
                            try
                            {
                                var args = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(toolCall.FunctionArguments);
                                if (args.TryGetProperty("sectionId", out var sectionIdProp) && Guid.TryParse(sectionIdProp.GetString(), out Guid sectionId))
                                {
                                    using var scope = _serviceProvider.CreateScope();
                                    var enrollmentService = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
                                    
                                    var result = await enrollmentService.RegisterForSectionAsync(studentCode, sectionId);
                                    
                                    if (result.IsSuccess)
                                    {
                                        toolOutputs.Add(new ToolOutput(toolCall.ToolCallId, "Đăng ký lớp thành công!"));
                                    }
                                    else
                                    {
                                        toolOutputs.Add(new ToolOutput(toolCall.ToolCallId, $"Lỗi khi đăng ký: {result.ErrorMessage}"));
                                    }
                                }
                                else
                                {
                                    toolOutputs.Add(new ToolOutput(toolCall.ToolCallId, "Lỗi: Tham số sectionId không hợp lệ."));
                                }
                            }
                            catch (Exception ex)
                            {
                                toolOutputs.Add(new ToolOutput(toolCall.ToolCallId, $"Lỗi hệ thống khi đăng ký: {ex.Message}"));
                            }
                        }
                    }
                    else
                    {
                        toolOutputs.Add(new ToolOutput(toolCall.ToolCallId, "Lỗi: Tool này hiện chưa được hỗ trợ."));
                    }
                }

                // Submit tool outputs back to the stream
                var toolStreamingResponse = assistantClient.SubmitToolOutputsToRunStreamingAsync(runUpdate.Value.ThreadId, runUpdate.Value.Id, toolOutputs, cancellationToken);
                await foreach (var toolUpdate in toolStreamingResponse)
                {
                    if (toolUpdate is MessageContentUpdate contentToolUpdate)
                    {
                        if (!string.IsNullOrEmpty(contentToolUpdate.Text))
                        {
                            yield return contentToolUpdate.Text;
                        }
                    }
                }
            }
        }
    }
}
