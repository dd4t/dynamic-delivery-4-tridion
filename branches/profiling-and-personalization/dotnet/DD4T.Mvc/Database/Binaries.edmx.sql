
-- Warning: There were errors validating the existing SSDL. Drop statements
-- will not be generated.
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 09/19/2011 11:48:02
-- Generated from EDMX file: D:\Development\DD4T\dotnet\DD4T.Mvc\Database\Binaries.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [CachingDatabase];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------


-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Binaries'
CREATE TABLE [dbo].[Binaries] (
    [ComponentUri] nvarchar(100)  NOT NULL,
    [Path] nvarchar(max)  NOT NULL,
    [Content] varbinary(max)  NOT NULL,
    [LastPublishedDate] datetime  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [ComponentUri] in table 'Binaries'
ALTER TABLE [dbo].[Binaries]
ADD CONSTRAINT [PK_Binaries]
    PRIMARY KEY CLUSTERED ([ComponentUri] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------