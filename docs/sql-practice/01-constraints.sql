-- ============================================================
-- 1. Database. GO ends a batch so the next statements run
--    after the database really exists.
-- ============================================================
IF DB_ID('FleetOptScratchDB') IS NULL
    CREATE DATABASE FleetOptScratchDB;
GO

USE FleetOptScratchDB;
GO

-- ============================================================
-- 2. Start clean (scratch data only). DROP ... IF EXISTS
--    makes the script re-runnable: you can run it 10 times.
-- ============================================================
DROP TABLE IF EXISTS dbo.Trucks;
GO

-- ============================================================
-- 3. Table with NAMED constraints
-- ============================================================
CREATE TABLE dbo.Trucks
(
    Id           INT            IDENTITY(1,1) NOT NULL,
    PlateNumber  NVARCHAR(20)   NOT NULL,
    MaxWeightKg  DECIMAL(10,2)  NOT NULL,
    CreatedAtUtc DATETIME2(0)   NOT NULL
    CONSTRAINT DF_Trucks_CreatedAtUtc DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Trucks             PRIMARY KEY (Id),
    CONSTRAINT UQ_Trucks_PlateNumber UNIQUE (PlateNumber),
    CONSTRAINT CK_Trucks_MaxWeightKg CHECK (MaxWeightKg > 0)
);
GO

-- ============================================================
-- 4. Valid data
-- ============================================================
INSERT INTO dbo.Trucks (PlateNumber, MaxWeightKg)
VALUES ('ABC123', 10000.00),
       ('XYZ789', 15000.00),
       ('LMN456', 12000.00);

SELECT * FROM dbo.Trucks;
GO

-- ============================================================
-- 5. Constraint tests: PREDICT the result before running each!
--    Select ONE statement and run only the selection.
-- ============================================================
-- Test A: duplicate plate
INSERT INTO dbo.Trucks (PlateNumber, MaxWeightKg) VALUES ('ABC123', 8000.00);

-- Test B: negative capacity
INSERT INTO dbo.Trucks (PlateNumber, MaxWeightKg) VALUES ('NEG001', -10);

-- Test C: try to sneak in a NULL timestamp
INSERT INTO dbo.Trucks (PlateNumber, MaxWeightKg, CreatedAtUtc) VALUES ('NUL001', 5000, NULL);
