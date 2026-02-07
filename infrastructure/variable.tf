variable "subscription_id" {
  description = "The Subscription ID to be used."
  type        = string
}

variable "resource_group_name" {
  description = "The name of the existing Resource Group."
  type        = string
}

variable "location" {
  description = "The Azure Region to be used."
  type        = string
  default     = "Southeast Asia"
}

variable "db_username" {
  description = "The username for the PostgreSQL database."
  type        = string
  validation {
    condition     = can(regex("^[a-zA-Z][a-zA-Z0-9]*$", var.db_username)) && !contains(["admin", "administrator", "root", "guest", "public", "azure_superuser"], lower(var.db_username))
    error_message = "The database username must start with a letter, contain only alphanumeric characters, and cannot be a reserved word (e.g., admin, root)."
  }
}

variable "db_password" {
  description = "The password for the PostgreSQL database."
  type        = string
  sensitive   = true
}
