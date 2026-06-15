REATE TABLE integration.TeamsNotifications (
    NotificationId      UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWSEQUENTIALID(),
    TenantId            UNIQUEIDENTIFIER  NOT NULL,
    NotificationType    NVARCHAR(100)     NOT NULL,
    RecipientType       NVARCHAR(50)      NOT NULL DEFAULT 'Channel',
    RecipientId         NVARCHAR(300)     NOT NULL,
    Subject             NVARCHAR(500)     NOT NULL,
    BodyJson            NVARCHAR(MAX)     NULL,
    RelatedEntityType   NVARCHAR(100)     NULL,
    RelatedEntityId     NVARCHAR(200)     NULL,
    Status              NVARCHAR(50)      NOT NULL DEFAULT 'Pending',
    SentAtUtc           DATETIME2(7)      NULL,
    ErrorMessage        NVARCHAR(1000)    NULL,
    RetryCount          TINYINT           NOT NULL DEFAULT 0,
    CreatedAtUtc        DATETIME2(7)      NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_TeamsNotifications PRIMARY KEY (NotificationId),
    CONSTRAINT FK_TeamsNotifications_Tenants FOREIGN KEY (TenantId) REFERENCES security.Tenants(TenantId),
    CONSTRAINT CK_TeamsNotifications_Status CHECK (Status IN ('Pending','Sent','Failed','Cancelled'))
);
GO
CREATE INDEX IX_TeamsNotifications_Status_CreatedAtUtc ON integration.TeamsNotifications(Status, CreatedAtUtc) WHERE Status = 'Pending';