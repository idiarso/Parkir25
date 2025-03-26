-- Add FirstName and LastName columns to AspNetUsers table
ALTER TABLE "AspNetUsers" ADD COLUMN "FirstName" text NULL DEFAULT '';
ALTER TABLE "AspNetUsers" ADD COLUMN "LastName" text NULL DEFAULT '';

-- Update the existing admin user
UPDATE "AspNetUsers" 
SET "FirstName" = 'System', "LastName" = 'Administrator'
WHERE "Email" = 'admin@parking.com'; 