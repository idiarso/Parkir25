-- Add CreatedAt column to AspNetUsers table
ALTER TABLE "AspNetUsers" ADD COLUMN "CreatedAt" timestamp with time zone NULL DEFAULT CURRENT_TIMESTAMP;
ALTER TABLE "AspNetUsers" ADD COLUMN "UpdatedAt" timestamp with time zone NULL;
ALTER TABLE "AspNetUsers" ADD COLUMN "ProfilePhotoPath" text NULL;
ALTER TABLE "AspNetUsers" ADD COLUMN "IsOperator" boolean NOT NULL DEFAULT false;
ALTER TABLE "AspNetUsers" ADD COLUMN "Notes" text NULL;

-- Update the existing admin user with these fields
UPDATE "AspNetUsers" 
SET "CreatedAt" = CURRENT_TIMESTAMP,
    "IsOperator" = true
WHERE "Email" = 'admin@parking.com'; 