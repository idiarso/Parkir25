-- Create Admin role
DELETE FROM "AspNetRoles" WHERE "Name" = 'Admin';

INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
VALUES ('1d4e1c5f-2b3a-4f6d-8e7c-9f0a1b2c3d4e', 'Admin', 'ADMIN', '5a6b7c8d-9e0f-1a2b-3c4d-5e6f7a8b9c0d');

-- Assign user to Admin role
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
VALUES ('8f7f4b1f-5a9e-4e6c-8d9d-3e7b6c3f8a9d', '1d4e1c5f-2b3a-4f6d-8e7c-9f0a1b2c3d4e')
ON CONFLICT DO NOTHING; 