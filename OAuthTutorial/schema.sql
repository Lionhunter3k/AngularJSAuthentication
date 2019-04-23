
    PRAGMA foreign_keys = OFF

    drop table if exists RoleClaim

    drop table if exists Role

    drop table if exists UserClaim

    drop table if exists UserLogin

    drop table if exists "User"

    drop table if exists UserRole

    drop table if exists UserToken

    drop table if exists OAuthClient

    drop table if exists RedirectURIs

    drop table if exists Token

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

    create table OAuthClient (
        ClientId TEXT not null,
       ClientSecret TEXT not null,
       Id TEXT not null,
       ClientName TEXT not null,
       ClientDescription TEXT not null,
       primary key (ClientId),
       constraint FK_ABC45D7D foreign key (Id) references "User"
    )

    create table RedirectURIs (
        ClientId TEXT not null,
       URI TEXT not null,
       constraint FK_FFF670E3 foreign key (ClientId) references OAuthClient
    )

    create table Token (
        TokenId INT not null,
       GrantType TEXT,
       TokenType TEXT,
       Value TEXT,
       ClientId TEXT not null,
       UserId TEXT not null,
       primary key (TokenId),
       constraint FK_445E5CED foreign key (ClientId) references OAuthClient,
       constraint FK_5B048BAC foreign key (UserId) references "User"
    )

    create table hibernate_unique_key (
         next_hi INT 
    )

    insert into hibernate_unique_key values ( 1 )
