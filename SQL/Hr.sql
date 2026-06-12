/*
 Navicat Premium Dump SQL

 Source Server         : 127.0.0.1
 Source Server Type    : SQL Server
 Source Server Version : 17001115 (17.00.1115)
 Source Host           : 127.0.0.1:1433
 Source Catalog        : SystemAdmin
 Source Schema         : Hr

 Target Server Type    : SQL Server
 Target Server Version : 17001115 (17.00.1115)
 File Encoding         : 65001

 Date: 12/06/2026 16:02:05
*/


-- ----------------------------
-- Table structure for UserLeaveAnnual
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Hr].[UserLeaveAnnual]') AND type IN ('U'))
	DROP TABLE [Hr].[UserLeaveAnnual]
GO

CREATE TABLE [Hr].[UserLeaveAnnual] (
  [UserId] bigint  NOT NULL,
  [Year] int  NOT NULL,
  [LeaveType] nvarchar(30) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [RenderDays] decimal(6,2)  NOT NULL,
  [RemainingDays] decimal(6,2)  NOT NULL,
  [CreatedBy] bigint  NOT NULL,
  [CreatedDate] datetime2(3)  NOT NULL,
  [ModifiedBy] bigint  NULL,
  [ModifiedDate] datetime2(3)  NULL
)
GO

ALTER TABLE [Hr].[UserLeaveAnnual] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'用户Id',
'SCHEMA', N'Hr',
'TABLE', N'UserLeaveAnnual',
'COLUMN', N'UserId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'年度',
'SCHEMA', N'Hr',
'TABLE', N'UserLeaveAnnual',
'COLUMN', N'Year'
GO

EXEC sp_addextendedproperty
'MS_Description', N'假别',
'SCHEMA', N'Hr',
'TABLE', N'UserLeaveAnnual',
'COLUMN', N'LeaveType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'给予天数',
'SCHEMA', N'Hr',
'TABLE', N'UserLeaveAnnual',
'COLUMN', N'RenderDays'
GO

EXEC sp_addextendedproperty
'MS_Description', N'给予天数',
'SCHEMA', N'Hr',
'TABLE', N'UserLeaveAnnual',
'COLUMN', N'RemainingDays'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'Hr',
'TABLE', N'UserLeaveAnnual',
'COLUMN', N'CreatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'Hr',
'TABLE', N'UserLeaveAnnual',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改人',
'SCHEMA', N'Hr',
'TABLE', N'UserLeaveAnnual',
'COLUMN', N'ModifiedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改时间',
'SCHEMA', N'Hr',
'TABLE', N'UserLeaveAnnual',
'COLUMN', N'ModifiedDate'
GO


-- ----------------------------
-- Records of UserLeaveAnnual
-- ----------------------------
INSERT INTO [Hr].[UserLeaveAnnual] ([UserId], [Year], [LeaveType], [RenderDays], [RemainingDays], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'1903486709602062340', N'2026', N'Annual', N'15.00', N'0.00', N'1903486709602062336', N'2026-06-11 15:02:30.000', NULL, NULL)
GO

