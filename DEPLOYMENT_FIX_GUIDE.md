# 🚀 Database Migration Fix - CompanyId in RolePermissions

## 📋 Problem Summary

**Issue:** The application was failing to start in production with the error:
```
ALTER TABLE ALTER COLUMN failed because column 'CompanyId' does not exist in table 'RolePermissions'.
```

**Root Cause:** 
- Migration `20260204172402_rolpercomp` was created but left **empty** (no SQL generated)
- The entity model expected `CompanyId` column, but it was never added to the database
- Subsequent migrations tried to ALTER a non-existent column

---

## ✅ What Was Fixed

### 1. **Fixed Empty Migration**
   - **File:** `20260204172402_rolpercomp.cs`
   - **Action:** Added the missing `ADD COLUMN` SQL to create the `CompanyId` column
   - **Details:**
     - Column is **nullable** (`int?`) - allows global permissions (CompanyId = NULL)
     - Includes foreign key to `Companies` table
     - Includes index for performance

### 2. **Updated Entity Model**
   - **File:** `RolePermission.cs`
   - **Action:** Made `Company` navigation property nullable
   - **Change:** `public virtual Company? Company { get; set; }`
   - **Reason:** Matches nullable `CompanyId` foreign key

### 3. **Updated DbContext Configuration**
   - **File:** `CoreDbContext.cs`
   - **Action:** Marked Company relationship as optional
   - **Change:** Added `.IsRequired(false)` to Company relationship
   - **Reason:** Explicitly tells EF Core that CompanyId can be NULL

---

## 🔧 What This Means

### **Global vs Company-Specific Permissions**

With `CompanyId` being nullable, you now have **two types of permissions**:

1. **Global Permissions** (CompanyId = NULL)
   - Apply to **all companies**
   - Used for system-wide roles (e.g., "Admin")
   - Default for seeded permissions

2. **Company-Specific Permissions** (CompanyId = specific value)
   - Apply only to **one company**
   - Used for multi-tenant scenarios
   - Can override global permissions

---

## 📦 Deployment Steps

### **For Fresh Deployments** (New database)
✅ **No action needed** - migrations will run automatically and create the column correctly.

### **For Existing Production Databases**

#### Option 1: Automatic (Recommended)
The fixed migration will run automatically on the next deployment:

```bash
# Just redeploy - migrations run on startup
# The rolpercomp migration will now create the CompanyId column
```

#### Option 2: Manual Migration (If automatic fails)

If you need to manually fix the database:

```sql
-- Connect to your production database
USE [your_database_name];

-- Add CompanyId column (nullable for global permissions)
ALTER TABLE [RolePermissions]
ADD [CompanyId] int NULL;

-- Create index for performance
CREATE NONCLUSTERED INDEX [IX_RolePermissions_CompanyId]
ON [RolePermissions]([CompanyId]);

-- Add foreign key constraint
ALTER TABLE [RolePermissions] WITH CHECK 
ADD CONSTRAINT [FK_RolePermissions_Companies_CompanyId] 
FOREIGN KEY([CompanyId])
REFERENCES [Companies] ([Id])
ON DELETE CASCADE;
```

Then mark the migration as applied:

```sql
-- Mark the migration as completed
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260204172402_rolpercomp', N'9.0.11');
```

---

## 🧪 Testing After Deployment

### 1. **Check Migration Success**
```sql
-- Verify CompanyId column exists
SELECT COLUMN_NAME, IS_NULLABLE, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'RolePermissions' AND COLUMN_NAME = 'CompanyId';

-- Should return: CompanyId | YES | int
```

### 2. **Check Seeded Data**
```sql
-- Check global permissions (CompanyId should be NULL)
SELECT TOP 5 
    rp.Id,
    r.Name AS RoleName,
    p.Code AS PermissionCode,
    rp.CompanyId
FROM RolePermissions rp
INNER JOIN Roles r ON rp.RoleId = r.Id
INNER JOIN Permissions p ON rp.PermissionId = p.Id;

-- CompanyId should be NULL for Admin role permissions
```

### 3. **Test Application**
```bash
# Check application logs for successful startup
# Should see: "Database migrations applied successfully."
```

---

## 🔍 Common Issues & Solutions

### Issue: "Migration already applied" error
**Solution:** The migration history is out of sync. Run the manual SQL above.

### Issue: "Duplicate index" error
**Solution:** Index already exists. Skip the CREATE INDEX step.

### Issue: "Foreign key conflict" error
**Solution:** 
```sql
-- Check if any orphaned records exist
SELECT * FROM RolePermissions 
WHERE CompanyId IS NOT NULL 
AND CompanyId NOT IN (SELECT Id FROM Companies);

-- Either delete orphaned records OR set to NULL (global permission)
UPDATE RolePermissions SET CompanyId = NULL 
WHERE CompanyId NOT IN (SELECT Id FROM Companies);
```

---

## 📊 Database Schema

### **Before Fix**
```
RolePermissions Table:
- Id (int, PK)
- RoleId (int, FK)
- PermissionId (int, FK)
- Read (bit)
- Write (bit)
- Update (bit)
- Delete (bit)
❌ CompanyId (MISSING!)
- ExpirationDate (datetime2, nullable)
```

### **After Fix**
```
RolePermissions Table:
- Id (int, PK)
- RoleId (int, FK)
- PermissionId (int, FK)
- Read (bit)
- Write (bit)
- Update (bit)
- Delete (bit)
✅ CompanyId (int, nullable, FK to Companies)
- ExpirationDate (datetime2, nullable)
```

---

## 🎯 Key Takeaways

1. ✅ **CompanyId is now properly added** to RolePermissions
2. ✅ **Nullable design** allows both global and company-specific permissions
3. ✅ **Migration fixed** - will work on both fresh and existing databases
4. ✅ **Backward compatible** - existing permissions will have CompanyId = NULL (global)

---

## 📞 Support

If you encounter any issues during deployment:

1. Check application logs for detailed error messages
2. Verify database schema with the SQL queries above
3. Ensure all migrations are applied: `SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId`
4. If needed, use manual SQL fix option

---

**Date:** February 6, 2026  
**Status:** ✅ Fixed and Ready for Deployment
