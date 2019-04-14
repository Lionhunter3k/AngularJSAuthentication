
    
alter table RoleClaim  drop foreign key FK_457BA18C


    
alter table UserClaim  drop foreign key FK_635765


    
alter table UserLogin  drop foreign key FK_9C81F253


    
alter table UserRole  drop foreign key FK_107ACC52


    
alter table UserRole  drop foreign key FK_449C7AF9


    
alter table UserToken  drop foreign key FK_7442E8F0


    
alter table RefreshToken2  drop foreign key FK_D9B6ABC4


    drop table if exists RoleClaim

    drop table if exists Role

    drop table if exists UserClaim

    drop table if exists UserLogin

    drop table if exists `User`

    drop table if exists UserRole

    drop table if exists UserToken

    drop table if exists RefreshToken2

    drop table if exists hibernate_unique_key

    create table RoleClaim (
        Id BIGINT not null,
       RoleId BIGINT not null,
       ClaimType VARCHAR(200) not null,
       ClaimValue VARCHAR(200) not null,
       primary key (Id)
    )

    create table Role (
        Id BIGINT not null,
       Name VARCHAR(200) not null unique,
       primary key (Id)
    )

    create table UserClaim (
        Id BIGINT not null,
       UserId VARCHAR(50) not null,
       ClaimType VARCHAR(200) not null,
       ClaimValue VARCHAR(200) not null,
       primary key (Id)
    )

    create table UserLogin (
        Id BIGINT not null,
       UserId VARCHAR(50) not null,
       LoginProvider VARCHAR(200) not null,
       ProviderKey VARCHAR(200) not null,
       ProviderDisplayName VARCHAR(200) not null,
       primary key (Id)
    )

    create table `User` (
        Id VARCHAR(50) not null,
       DisplayName VARCHAR(50),
       Email VARCHAR(50) not null,
       EmailConfirmed TINYINT(1),
       EmailConfirmedOnUtc DATETIME(6),
       PhoneNumber VARCHAR(20) not null,
       PhoneNumberConfirmed TINYINT(1),
       PhoneNumberConfirmedOnUtc DATETIME(6),
       PasswordHash VARCHAR(200) not null,
       SecurityStamp VARCHAR(50) not null,
       Deleted TINYINT(1),
       AccessFailedCount INTEGER,
       LockoutEnabled TINYINT(1),
       LockoutEndUtc DATETIME(6),
       TwoFactorEnabled TINYINT(1),
       primary key (Id)
    )

    create table UserRole (
        UserId VARCHAR(50) not null,
       RoleId BIGINT not null,
       primary key (UserId, RoleId)
    )

    create table UserToken (
        Id BIGINT not null,
       UserId VARCHAR(50) not null,
       LoginProvider VARCHAR(200) not null,
       Name VARCHAR(200) not null,
       Value VARCHAR(200) not null,
       primary key (Id)
    )

    create table RefreshToken2 (
        Id CHAR(36) not null,
       Token TEXT not null,
       ExpiresUtc DATETIME(6) not null,
       UserId VARCHAR(50) not null,
       RemoteIpAddress VARCHAR(255) not null,
       ClientId TEXT not null,
       primary key (Id)
    )

    alter table RoleClaim 
        add index (RoleId), 
        add constraint FK_457BA18C 
        foreign key (RoleId) 
        references Role (Id)

    alter table UserClaim 
        add index (UserId), 
        add constraint FK_635765 
        foreign key (UserId) 
        references `User` (Id)

    alter table UserLogin 
        add index (UserId), 
        add constraint FK_9C81F253 
        foreign key (UserId) 
        references `User` (Id)

    alter table UserRole 
        add index (RoleId), 
        add constraint FK_107ACC52 
        foreign key (RoleId) 
        references Role (Id)

    alter table UserRole 
        add index (UserId), 
        add constraint FK_449C7AF9 
        foreign key (UserId) 
        references `User` (Id)

    alter table UserToken 
        add index (UserId), 
        add constraint FK_7442E8F0 
        foreign key (UserId) 
        references `User` (Id)

    alter table RefreshToken2 
        add index (UserId), 
        add constraint FK_D9B6ABC4 
        foreign key (UserId) 
        references `User` (Id)

    create table hibernate_unique_key (
         next_hi BIGINT 
    )

    insert into hibernate_unique_key values ( 1 )
