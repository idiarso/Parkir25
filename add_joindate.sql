-- Add JoinDate column to AspNetUsers table
ALTER TABLE "AspNetUsers" ADD COLUMN "JoinDate" timestamp with time zone NULL DEFAULT CURRENT_TIMESTAMP;

-- Update the existing admin user
UPDATE "AspNetUsers" 
SET "JoinDate" = CURRENT_TIMESTAMP
WHERE "Email" = 'admin@parking.com'; 