-- Script to fix the TotalAmount column type in the Bookings table
-- First backup existing data
SELECT * INTO BookingsBackup FROM Bookings;

-- Check the current column type
PRINT 'Current column type for TotalAmount: '
SELECT DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Bookings' AND COLUMN_NAME = 'TotalAmount';

-- Convert decimal data to integer
BEGIN TRY
    BEGIN TRANSACTION;
    
    -- Create a temporary column
    ALTER TABLE Bookings ADD TotalAmount_Int INT NULL;
    
    -- Update the temporary column with converted values
    UPDATE Bookings SET TotalAmount_Int = CAST(TotalAmount AS INT);
    
    -- Drop the original column
    ALTER TABLE Bookings DROP COLUMN TotalAmount;
    
    -- Rename the temp column to the original name
    EXEC sp_rename 'Bookings.TotalAmount_Int', 'TotalAmount', 'COLUMN';
    
    -- Make the column NOT NULL again
    ALTER TABLE Bookings ALTER COLUMN TotalAmount INT NOT NULL;
    
    COMMIT TRANSACTION;
    PRINT 'Successfully changed TotalAmount column from DECIMAL to INT';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Error occurred: ' + ERROR_MESSAGE();
END CATCH

-- Verify the new column type
PRINT 'New column type for TotalAmount: '
SELECT DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Bookings' AND COLUMN_NAME = 'TotalAmount'; 