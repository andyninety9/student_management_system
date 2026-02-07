output "postgres_server_host" {
  value = azurerm_postgresql_flexible_server.default.fqdn
}

output "postgres_database_name" {
  value = azurerm_postgresql_flexible_server_database.default.name
}

output "redis_hostname" {
  value = azurerm_redis_cache.default.hostname
}

output "redis_ssl_port" {
  value = azurerm_redis_cache.default.ssl_port
}
