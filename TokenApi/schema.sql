
    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_457BA18C]') and parent_object_id = OBJECT_ID(N'RoleClaim'))
alter table RoleClaim  drop constraint FK_457BA18C


    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_635765]') and parent_object_id = OBJECT_ID(N'UserClaim'))
alter table UserClaim  drop constraint FK_635765


    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_9C81F253]') and parent_object_id = OBJECT_ID(N'UserLogin'))
alter table UserLogin  drop constraint FK_9C81F253


    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_107ACC52]') and parent_object_id = OBJECT_ID(N'UserRole'))
alter table UserRole  drop constraint FK_107ACC52


    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_449C7AF9]') and parent_object_id = OBJECT_ID(N'UserRole'))
alter table UserRole  drop constraint FK_449C7AF9


    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_7442E8F0]') and parent_object_id = OBJECT_ID(N'UserToken'))
alter table UserToken  drop constraint FK_7442E8F0


    if exists (select 1 from sys.objects where object_id = OBJECT_ID(N'[FK_7B1D97F9]') and parent_object_id = OBJECT_ID(N'RefreshToken'))
alter table RefreshToken  drop constraint FK_7B1D97F9


    if exists (select * from dbo.sysobjects where id = object_id(N'RoleClaim') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table RoleClaim

    if exists (select * from dbo.sysobjects where id = object_id(N'Role') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table Role

    if exists (select * from dbo.sysobjects where id = object_id(N'UserClaim') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table UserClaim

    if exists (select * from dbo.sysobjects where id = object_id(N'UserLogin') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table UserLogin

    if exists (select * from dbo.sysobjects where id = object_id(N'[User]') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table [User]

    if exists (select * from dbo.sysobjects where id = object_id(N'UserRole') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table UserRole

    if exists (select * from dbo.sysobjects where id = object_id(N'UserToken') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table UserToken

    if exists (select * from dbo.sysobjects where id = object_id(N'RefreshToken') and OBJECTPROPERTY(id, N'IsUserTable') = 1) drop table RefreshToken

    IF EXISTS (SELECT * FROM sys.sequences WHERE object_id = OBJECT_ID(N'hibernate_sequence')) DROP SEQUENCE hibernate_sequence

    create table RoleClaim (
        Id BIGINT not null,
       RoleId BIGINT not null,
       ClaimType NVARCHAR(200) not null,
       ClaimValue NVARCHAR(200) not null,
       primary key (Id)
    )

    create table Role (
        Id BIGINT not null,
       Name NVARCHAR(200) not null unique,
       primary key (Id)
    )

    create table UserClaim (
        Id BIGINT not null,
       UserId NVARCHAR(50) not null,
       ClaimType NVARCHAR(200) not null,
       ClaimValue NVARCHAR(200) not null,
       primary key (Id)
    )

    create table UserLogin (
        Id BIGINT not null,
       UserId NVARCHAR(50) not null,
       LoginProvider NVARCHAR(200) not null,
       ProviderKey NVARCHAR(200) not null,
       ProviderDisplayName NVARCHAR(200) not null,
       primary key (Id)
    )

    create table [User] (
        Id NVARCHAR(50) not null,
       DisplayName NVARCHAR(50) null,
       Email NVARCHAR(50) not null,
       EmailConfirmed BIT null,
       EmailConfirmedOnUtc DATETIME2 null,
       PhoneNumber NVARCHAR(20) not null,
       PhoneNumberConfirmed BIT null,
       PhoneNumberConfirmedOnUtc DATETIME2 null,
       PasswordHash NVARCHAR(200) not null,
       SecurityStamp NVARCHAR(50) not null,
       Deleted BIT null,
       AccessFailedCount INT null,
       LockoutEnabled BIT null,
       LockoutEndUtc DATETIME2 null,
       TwoFactorEnabled BIT null,
       primary key (Id)
    )

    create table UserRole (
        UserId NVARCHAR(50) not null,
       RoleId BIGINT not null,
       primary key (UserId, RoleId)
    )

    create table UserToken (
        Id BIGINT not null,
       UserId NVARCHAR(50) not null,
       LoginProvider NVARCHAR(200) not null,
       Name NVARCHAR(200) not null,
       Value NVARCHAR(200) not null,
       primary key (Id)
    )

    create table RefreshToken (
        Id BIGINT not null,
       IssuedUtc DATETIME2 not null,
       ExpiresUtc DATETIME2 not null,
       Token NVARCHAR(1000) not null,
       UserId NVARCHAR(50) not null,
       Type NVARCHAR(1000) not null,
       primary key (Id)
    )

    alter table RoleClaim
        add constraint FK_457BA18C
        foreign key (RoleId)
        references Role

    alter table UserClaim
        add constraint FK_635765
        foreign key (UserId)
        references [User]

    alter table UserLogin
        add constraint FK_9C81F253
        foreign key (UserId)
        references [User]

    alter table UserRole
        add constraint FK_107ACC52
        foreign key (RoleId)
        references Role

    alter table UserRole
        add constraint FK_449C7AF9
        foreign key (UserId)
        references [User]

    alter table UserToken
        add constraint FK_7442E8F0
        foreign key (UserId)
        references [User]

    alter table RefreshToken
        add constraint FK_7B1D97F9
        foreign key (UserId)
        references [User]

    create sequence hibernate_sequence as int start with 1 increment by 1
