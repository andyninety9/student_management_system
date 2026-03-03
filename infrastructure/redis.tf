resource "azurerm_redis_cache" "default" {
  name                = "redis-sms-dev-${random_string.suffix.result}"
  location            = var.location
  resource_group_name = data.azurerm_resource_group.existing_rg.name
  capacity            = 0
  family              = "C"
  sku_name            = "Basic"
  non_ssl_port_enabled = false
  minimum_tls_version = "1.2"

  redis_configuration {
  }
}
