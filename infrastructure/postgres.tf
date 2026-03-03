resource "azurerm_postgresql_flexible_server" "default" {
  name                   = "psql-sms-dev-${random_string.suffix.result}"
  resource_group_name    = data.azurerm_resource_group.existing_rg.name
  location               = var.location
  version                = "13"
  administrator_login    = var.db_username
  administrator_password = var.db_password
  storage_mb             = 32768
  sku_name               = "B_Standard_B1ms"
  
  # "B_Standard_B1ms" is one of the lowest cost options for Flexible Server (Burstable).
  # Use "GP_Standard_D2s_v3" if you need General Purpose.
}

resource "azurerm_postgresql_flexible_server_database" "default" {
  name      = "sms_db"
  server_id = azurerm_postgresql_flexible_server.default.id
  collation = "en_US.utf8"
  charset   = "utf8"
}

# Allow access from Azure services
resource "azurerm_postgresql_flexible_server_firewall_rule" "allow_azure_services" {
  name             = "allow-azure-services"
  server_id        = azurerm_postgresql_flexible_server.default.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

# Allow access from all IPs (FOR DEV/TEST ONLY - NOT SECURE FOR PROD)
# You might want to restrict this to your specific IP address in a real scenario.
resource "azurerm_postgresql_flexible_server_firewall_rule" "allow_all" {
  name             = "allow-all"
  server_id        = azurerm_postgresql_flexible_server.default.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "255.255.255.255"
}
