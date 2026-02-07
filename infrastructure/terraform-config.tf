terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.0"
    }
  }
}

provider "azurerm" {
  features {}
  subscription_id = var.subscription_id
}

data "azurerm_resource_group" "existing_rg" {
  name = var.resource_group_name
}

# Example of how to use the existing resource group
output "resource_group_id" {
  value = data.azurerm_resource_group.existing_rg.id
}
