CREATE TABLE "DryCleanServiceDefinition" (id TEXT PRIMARY KEY, name TEXT NOT NULL COLLATE NOCASE UNIQUE, isActive INTEGER NOT NULL DEFAULT 1, createdAt TEXT NOT NULL, updatedAt TEXT NOT NULL);
CREATE INDEX idx_dry_clean_services_active ON DryCleanServiceDefinition(isActive, name);
ALTER TABLE DryCleanCatalogItem ADD COLUMN serviceId TEXT NULL;
CREATE INDEX idx_dry_clean_catalog_service ON DryCleanCatalogItem(serviceId);
