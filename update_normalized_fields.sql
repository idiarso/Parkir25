-- Update normalized fields for admin user
UPDATE "AspNetUsers" 
SET "NormalizedUserName" = 'ADMIN@PARKING.COM',
    "NormalizedEmail" = 'ADMIN@PARKING.COM',
    "UserName" = 'admin@parking.com',
    "EmailConfirmed" = true
WHERE "Email" = 'admin@parking.com'; 