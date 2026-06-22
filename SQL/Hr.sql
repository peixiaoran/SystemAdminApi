/*
 Navicat Premium Dump SQL

 Source Server         : 127.0.0.1
 Source Server Type    : SQL Server
 Source Server Version : 17001115 (17.00.1115)
 Source Host           : localhost:1433
 Source Catalog        : SystemAdmin
 Source Schema         : Hr

 Target Server Type    : SQL Server
 Target Server Version : 17001115 (17.00.1115)
 File Encoding         : 65001

 Date: 22/06/2026 20:08:47
*/


-- ----------------------------
-- Table structure for UserLeaveBalance
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Hr].[UserLeaveBalance]') AND type IN ('U'))
	DROP TABLE [Hr].[UserLeaveBalance]
GO

CREATE TABLE [Hr].[UserLeaveBalance] (
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

ALTER TABLE [Hr].[UserLeaveBalance] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'用户Id',
'SCHEMA', N'Hr',
'TABLE', N'UserLeaveBalance',
'COLUMN', N'UserId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'年度',
'SCHEMA', N'Hr',
'TABLE', N'UserLeaveBalance',
'COLUMN', N'Year'
GO

EXEC sp_addextendedproperty
'MS_Description', N'假别',
'SCHEMA', N'Hr',
'TABLE', N'UserLeaveBalance',
'COLUMN', N'LeaveType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'给予天数',
'SCHEMA', N'Hr',
'TABLE', N'UserLeaveBalance',
'COLUMN', N'RenderDays'
GO

EXEC sp_addextendedproperty
'MS_Description', N'给予天数',
'SCHEMA', N'Hr',
'TABLE', N'UserLeaveBalance',
'COLUMN', N'RemainingDays'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'Hr',
'TABLE', N'UserLeaveBalance',
'COLUMN', N'CreatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'Hr',
'TABLE', N'UserLeaveBalance',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改人',
'SCHEMA', N'Hr',
'TABLE', N'UserLeaveBalance',
'COLUMN', N'ModifiedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改时间',
'SCHEMA', N'Hr',
'TABLE', N'UserLeaveBalance',
'COLUMN', N'ModifiedDate'
GO


-- ----------------------------
-- Records of UserLeaveBalance
-- ----------------------------
INSERT INTO [Hr].[UserLeaveBalance] ([UserId], [Year], [LeaveType], [RenderDays], [RemainingDays], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'1903486709602062336', N'2026', N'Annual', N'15.00', N'11.00', N'1903486709602062336', N'2026-06-11 15:02:30.000', N'1903486709602062336', N'2026-06-22 20:00:36.043')
GO

INSERT INTO [Hr].[UserLeaveBalance] ([UserId], [Year], [LeaveType], [RenderDays], [RemainingDays], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'1903486709602062336', N'2026', N'Sick', N'20.00', N'20.00', N'1903486709602062336', N'2026-06-11 15:02:30.000', NULL, NULL)
GO

INSERT INTO [Hr].[UserLeaveBalance] ([UserId], [Year], [LeaveType], [RenderDays], [RemainingDays], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'1903486709602062336', N'2027', N'Annual', N'15.00', N'15.00', N'1903486709602062336', N'2026-06-11 15:02:30.000', NULL, NULL)
GO

INSERT INTO [Hr].[UserLeaveBalance] ([UserId], [Year], [LeaveType], [RenderDays], [RemainingDays], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'1903486709602062336', N'2027', N'Sick', N'20.00', N'20.00', N'1903486709602062336', N'2026-06-11 15:02:30.000', NULL, NULL)
GO

