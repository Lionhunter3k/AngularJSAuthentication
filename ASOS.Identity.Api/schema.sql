
    PRAGMA foreign_keys = OFF

    drop table if exists RoleClaim

    drop table if exists Role

    drop table if exists UserClaim

    drop table if exists UserLogin

    drop table if exists "User"

    drop table if exists UserRole

    drop table if exists UserToken

    drop table if exists "ClientApplication"

    drop table if exists AllowedGrants

    drop table if exists AllowedRedirectUris

    drop table if exists hibernate_unique_key

    PRAGMA foreign_keys = ON

    create table RoleClaim (
        Id BIGINT not null,
       RoleId BIGINT not null,
       ClaimType TEXT not null,
       ClaimValue TEXT not null,
       primary key (Id),
       constraint FK_457BA18C foreign key (RoleId) references Role
    )

    create table Role (
        Id BIGINT not null,
       Name TEXT not null unique,
       primary key (Id)
    )

    create table UserClaim (
        Id BIGINT not null,
       UserId TEXT not null,
       ClaimType TEXT not null,
       ClaimValue TEXT not null,
       primary key (Id),
       constraint FK_635765 foreign key (UserId) references "User"
    )

    create table UserLogin (
        Id BIGINT not null,
       UserId TEXT not null,
       LoginProvider TEXT not null,
       ProviderKey TEXT not null,
       ProviderDisplayName TEXT not null,
       primary key (Id),
       constraint FK_9C81F253 foreign key (UserId) references "User"
    )

    create table "User" (
        Id TEXT not null,
       DisplayName TEXT,
       Email TEXT not null,
       EmailConfirmed BOOL,
       EmailConfirmedOnUtc DATETIME,
       PhoneNumber TEXT not null,
       PhoneNumberConfirmed BOOL,
       PhoneNumberConfirmedOnUtc DATETIME,
       PasswordHash TEXT,
       SecurityStamp TEXT not null,
       Deleted BOOL,
       AccessFailedCount INT,
       LockoutEnabled BOOL,
       LockoutEndUtc DATETIME,
       TwoFactorEnabled BOOL,
       primary key (Id)
    )

    create table UserRole (
        UserId TEXT not null,
       RoleId BIGINT not null,
       primary key (UserId, RoleId),
       constraint FK_107ACC52 foreign key (RoleId) references Role,
       constraint FK_449C7AF9 foreign key (UserId) references "User"
    )

    create table UserToken (
        Id BIGINT not null,
       UserId TEXT not null,
       LoginProvider TEXT not null,
       Name TEXT not null,
       Value TEXT not null,
       primary key (Id),
       constraint FK_7442E8F0 foreign key (UserId) references "User"
    )

    create table "ClientApplication" (
        Id TEXT not null,
       Type INT,
       Secret TEXT,
       primary key (Id)
    )

    create table AllowedGrants (
        clientapplication_key TEXT not null,
       GrantType TEXT not null,
       GrantIndex INT not null,
       primary key (clientapplication_key, GrantIndex),
       constraint FK_ED35FA44 foreign key (clientapplication_key) references "ClientApplication"
    )

    create table AllowedRedirectUris (
        clientapplication_key TEXT not null,
       RedirectUri TEXT not null,
       RedirectUriIndex INT not null,
       primary key (clientapplication_key, RedirectUriIndex),
       constraint FK_AAA35964 foreign key (clientapplication_key) references "ClientApplication"
    )

    create table hibernate_unique_key (
         next_hi BIGINT 
    )

    insert into hibernate_unique_key values ( 1 )
