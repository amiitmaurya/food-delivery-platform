using Microsoft.EntityFrameworkCore;
using MiniSwiggy.Infrastructure.Data;
using System;
using System.Threading.Tasks;

namespace MiniSwiggy.Infrastructure.Seed;

public static class DeliveryPartnerTableSeeder
{
    public static async Task EnsureTablesExistAsync(ApplicationDbContext context)
    {
        try
        {
            var sql = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DeliveryPartnerProfiles')
                BEGIN
                    CREATE TABLE [DeliveryPartnerProfiles] (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [UserId] INT NOT NULL,
                        [IsOnline] BIT NOT NULL DEFAULT 1,
                        [VehicleType] NVARCHAR(MAX) NOT NULL DEFAULT 'Bike',
                        [VehicleNumber] NVARCHAR(MAX) NOT NULL DEFAULT '',
                        [VehicleModel] NVARCHAR(MAX) NOT NULL DEFAULT '',
                        [LicenseNumber] NVARCHAR(MAX) NOT NULL DEFAULT '',
                        [LicenseExpiryDate] NVARCHAR(MAX) NOT NULL DEFAULT '',
                        [BankAccountHolder] NVARCHAR(MAX) NOT NULL DEFAULT '',
                        [BankName] NVARCHAR(MAX) NOT NULL DEFAULT '',
                        [AccountNumber] NVARCHAR(MAX) NOT NULL DEFAULT '',
                        [IfscCode] NVARCHAR(MAX) NOT NULL DEFAULT '',
                        [UpiId] NVARCHAR(MAX) NOT NULL DEFAULT '',
                        [Rating] FLOAT NOT NULL DEFAULT 4.8,
                        [TotalRatingsCount] INT NOT NULL DEFAULT 12,
                        [CreatedOn] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                        [UpdatedOn] DATETIME2 NULL,
                        [IsDeleted] BIT NOT NULL DEFAULT 0
                    );
                END
                ELSE
                BEGIN
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DeliveryPartnerProfiles') AND name = 'CreatedDate')
                    BEGIN
                        EXEC sp_rename 'DeliveryPartnerProfiles.CreatedDate', 'CreatedOn', 'COLUMN';
                    END;
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DeliveryPartnerProfiles') AND name = 'UpdatedDate')
                    BEGIN
                        EXEC sp_rename 'DeliveryPartnerProfiles.UpdatedDate', 'UpdatedOn', 'COLUMN';
                    END;
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DeliveryPartnerProfiles') AND name = 'CreatedOn')
                    BEGIN
                        ALTER TABLE [DeliveryPartnerProfiles] ADD [CreatedOn] DATETIME2 NOT NULL DEFAULT GETUTCDATE();
                    END;
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DeliveryPartnerProfiles') AND name = 'UpdatedOn')
                    BEGIN
                        ALTER TABLE [DeliveryPartnerProfiles] ADD [UpdatedOn] DATETIME2 NULL;
                    END;
                END;

                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DeliveryOrderTrackers')
                BEGIN
                    CREATE TABLE [DeliveryOrderTrackers] (
                        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                        [OrderId] INT NOT NULL,
                        [DeliveryPartnerUserId] INT NOT NULL,
                        [DeliveryStatus] NVARCHAR(MAX) NOT NULL DEFAULT 'Assigned',
                        [AssignedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                        [AcceptedAt] DATETIME2 NULL,
                        [ReachedRestaurantAt] DATETIME2 NULL,
                        [PickedUpAt] DATETIME2 NULL,
                        [OutForDeliveryAt] DATETIME2 NULL,
                        [DeliveredAt] DATETIME2 NULL,
                        [CustomerRating] FLOAT NULL,
                        [CustomerFeedback] NVARCHAR(MAX) NULL,
                        [DeliveryEarnings] DECIMAL(18,2) NOT NULL DEFAULT 40.00,
                        [CreatedOn] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                        [UpdatedOn] DATETIME2 NULL,
                        [IsDeleted] BIT NOT NULL DEFAULT 0
                    );
                END
                ELSE
                BEGIN
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DeliveryOrderTrackers') AND name = 'CreatedDate')
                    BEGIN
                        EXEC sp_rename 'DeliveryOrderTrackers.CreatedDate', 'CreatedOn', 'COLUMN';
                    END;
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DeliveryOrderTrackers') AND name = 'UpdatedDate')
                    BEGIN
                        EXEC sp_rename 'DeliveryOrderTrackers.UpdatedDate', 'UpdatedOn', 'COLUMN';
                    END;
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DeliveryOrderTrackers') AND name = 'CreatedOn')
                    BEGIN
                        ALTER TABLE [DeliveryOrderTrackers] ADD [CreatedOn] DATETIME2 NOT NULL DEFAULT GETUTCDATE();
                    END;
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('DeliveryOrderTrackers') AND name = 'UpdatedOn')
                    BEGIN
                        ALTER TABLE [DeliveryOrderTrackers] ADD [UpdatedOn] DATETIME2 NULL;
                    END;
                END;

                -- Cleanup duplicate trackers for the same order ID keeping the row with highest progress/timestamps
                WITH CTE AS (
                    SELECT Id, OrderId,
                           ROW_NUMBER() OVER (
                               PARTITION BY OrderId 
                               ORDER BY 
                                   CASE WHEN DeliveredAt IS NOT NULL THEN 1 ELSE 2 END,
                                   CASE WHEN OutForDeliveryAt IS NOT NULL THEN 1 ELSE 2 END,
                                   CASE WHEN PickedUpAt IS NOT NULL THEN 1 ELSE 2 END,
                                   CASE WHEN ReachedRestaurantAt IS NOT NULL THEN 1 ELSE 2 END,
                                   CASE WHEN AcceptedAt IS NOT NULL THEN 1 ELSE 2 END,
                                   Id ASC
                           ) AS RowNum
                    FROM DeliveryOrderTrackers
                )
                DELETE FROM DeliveryOrderTrackers
                WHERE Id IN (SELECT Id FROM CTE WHERE RowNum > 1);
            ";

            await context.Database.ExecuteSqlRawAsync(sql);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DeliveryPartnerTableSeeder] Notice: {ex.Message}");
        }
    }
}
