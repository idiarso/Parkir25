INSERT INTO "AspNetUsers" (
    "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
    "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount",
    "FullName", "Name", "IsOnDuty", "IsActive")
VALUES (
    '8f7f4b1f-5a9e-4e6c-8d9d-3e7b6c3f8a9d', 'admin@parking.com', 'ADMIN@PARKING.COM', 'admin@parking.com', 'ADMIN@PARKING.COM',
    TRUE, 'AQAAAAEAACcQAAAAEPe6RjJ5EbJQ91JEa6Qr7t99L0VilUQkM02x3p/pCz+EJL+oO7LYBcvI5D8kkRPUiA==',
    'TNLRJJLNKIOFMQXSZEJZPHKGXGBPNCAD', '4a8c907c-3f07-4c9a-9b5e-4c0d7c3d7e8f',
    FALSE, FALSE, FALSE, 0,
    'System Administrator', 'Administrator', FALSE, TRUE
);

-- Create Admin role if it doesn't exist
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
VALUES ('1d4e1c5f-2b3a-4f6d-8e7c-9f0a1b2c3d4e', 'Admin', 'ADMIN', '5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c0d')
ON CONFLICT DO NOTHING;

-- Assign user to Admin role
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
VALUES ('8f7f4b1f-5a9e-4e6c-8d9d-3e7b6c3f8a9d', '1d4e1c5f-2b3a-4f6d-8e7c-9f0a1b2c3d4e')
ON CONFLICT DO NOTHING; 