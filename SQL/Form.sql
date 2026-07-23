/*
 Navicat Premium Dump SQL

 Source Server         : 127.0.0.1
 Source Server Type    : SQL Server
 Source Server Version : 17001125 (17.00.1125)
 Source Host           : localhost:1433
 Source Catalog        : SystemAdmin
 Source Schema         : Form

 Target Server Type    : SQL Server
 Target Server Version : 17001125 (17.00.1125)
 File Encoding         : 65001

 Date: 23/07/2026 22:50:54
*/


-- ----------------------------
-- Table structure for ControlInfo
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Form].[ControlInfo]') AND type IN ('U'))
	DROP TABLE [Form].[ControlInfo]
GO

CREATE TABLE [Form].[ControlInfo] (
  [ControlCode] nvarchar(20) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [ControlName] nvarchar(30) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [Description] nvarchar(100) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NULL,
  [CreatedBy] bigint  NOT NULL,
  [CreatedDate] datetime DEFAULT getdate() NOT NULL,
  [ModifiedBy] bigint  NULL,
  [ModifiedDate] datetime  NULL
)
GO

ALTER TABLE [Form].[ControlInfo] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'控件类型编码',
'SCHEMA', N'Form',
'TABLE', N'ControlInfo',
'COLUMN', N'ControlCode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'控件类型名称',
'SCHEMA', N'Form',
'TABLE', N'ControlInfo',
'COLUMN', N'ControlName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'控件类型描述',
'SCHEMA', N'Form',
'TABLE', N'ControlInfo',
'COLUMN', N'Description'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'Form',
'TABLE', N'ControlInfo',
'COLUMN', N'CreatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'Form',
'TABLE', N'ControlInfo',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改人',
'SCHEMA', N'Form',
'TABLE', N'ControlInfo',
'COLUMN', N'ModifiedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改时间',
'SCHEMA', N'Form',
'TABLE', N'ControlInfo',
'COLUMN', N'ModifiedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'控件信息表',
'SCHEMA', N'Form',
'TABLE', N'ControlInfo'
GO


-- ----------------------------
-- Records of ControlInfo
-- ----------------------------
INSERT INTO [Form].[ControlInfo] ([ControlCode], [ControlName], [Description], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'el-button', N'el-button', N'按钮', N'1903486709602062336', N'2025-11-05 16:02:49.000', NULL, NULL)
GO

INSERT INTO [Form].[ControlInfo] ([ControlCode], [ControlName], [Description], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'el-checkbox', N'el-checkbox', N'单个多选框，返回true、false', N'1903486709602062336', N'2025-11-05 13:26:29.000', NULL, NULL)
GO

INSERT INTO [Form].[ControlInfo] ([ControlCode], [ControlName], [Description], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'el-checkbox-group', N'el-checkbox-group', N'组多选框，返回数组', N'1903486709602062336', N'2025-11-05 13:27:29.000', NULL, NULL)
GO

INSERT INTO [Form].[ControlInfo] ([ControlCode], [ControlName], [Description], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'el-date-picker', N'el-date-picker', N'日期选择框', N'1903486709602062336', N'2025-11-05 13:30:56.000', NULL, NULL)
GO

INSERT INTO [Form].[ControlInfo] ([ControlCode], [ControlName], [Description], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'el-input', N'el-input', N'输入框', N'1903486709602062336', N'2025-11-04 16:33:36.000', NULL, NULL)
GO

INSERT INTO [Form].[ControlInfo] ([ControlCode], [ControlName], [Description], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'el-input-number', N'el-input-number', N'数值输入框', N'1903486709602062336', N'2025-11-04 16:33:38.000', NULL, NULL)
GO

INSERT INTO [Form].[ControlInfo] ([ControlCode], [ControlName], [Description], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'el-radio-group', N'el-radio-group', N'组单选框', N'1903486709602062336', N'2025-11-05 13:26:17.000', NULL, NULL)
GO

INSERT INTO [Form].[ControlInfo] ([ControlCode], [ControlName], [Description], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'el-select', N'el-select', N'下拉框', N'1903486709602062336', N'2025-11-05 11:58:47.000', NULL, NULL)
GO

INSERT INTO [Form].[ControlInfo] ([ControlCode], [ControlName], [Description], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'el-switch', N'el-switch', N'开关', N'1903486709602062336', N'2025-11-09 00:13:35.000', NULL, NULL)
GO


-- ----------------------------
-- Table structure for FormAttachment
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Form].[FormAttachment]') AND type IN ('U'))
	DROP TABLE [Form].[FormAttachment]
GO

CREATE TABLE [Form].[FormAttachment] (
  [AttachmentId] bigint  NOT NULL,
  [FormId] bigint  NOT NULL,
  [AttachmentName] nvarchar(50) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [AttachmentPath] nvarchar(100) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [AttachmentSize] int  NOT NULL,
  [CreatedBy] bigint  NULL,
  [CreatedDate] datetime2(3)  NULL
)
GO

ALTER TABLE [Form].[FormAttachment] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'附件Id',
'SCHEMA', N'Form',
'TABLE', N'FormAttachment',
'COLUMN', N'AttachmentId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单Id',
'SCHEMA', N'Form',
'TABLE', N'FormAttachment',
'COLUMN', N'FormId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'附件文件名',
'SCHEMA', N'Form',
'TABLE', N'FormAttachment',
'COLUMN', N'AttachmentName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'附件相对路径',
'SCHEMA', N'Form',
'TABLE', N'FormAttachment',
'COLUMN', N'AttachmentPath'
GO

EXEC sp_addextendedproperty
'MS_Description', N'附件大小（kb）',
'SCHEMA', N'Form',
'TABLE', N'FormAttachment',
'COLUMN', N'AttachmentSize'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'Form',
'TABLE', N'FormAttachment',
'COLUMN', N'CreatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'Form',
'TABLE', N'FormAttachment',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单附件表',
'SCHEMA', N'Form',
'TABLE', N'FormAttachment'
GO


-- ----------------------------
-- Records of FormAttachment
-- ----------------------------
INSERT INTO [Form].[FormAttachment] ([AttachmentId], [FormId], [AttachmentName], [AttachmentPath], [AttachmentSize], [CreatedBy], [CreatedDate]) VALUES (N'1', N'2079842855760826368', N'1', N'1', N'1', N'1', N'2026-07-23 15:53:40.000')
GO


-- ----------------------------
-- Table structure for FormGroup
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Form].[FormGroup]') AND type IN ('U'))
	DROP TABLE [Form].[FormGroup]
GO

CREATE TABLE [Form].[FormGroup] (
  [FormGroupId] bigint  NOT NULL,
  [FormGroupNameCn] nvarchar(20) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [FormGroupNameEn] nvarchar(50) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [SortOrder] int  NOT NULL,
  [DescriptionCn] nvarchar(300) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NULL,
  [DescriptionEn] nvarchar(500) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NULL,
  [CreatedBy] bigint  NULL,
  [CreatedDate] datetime DEFAULT getdate() NULL,
  [ModifiedBy] bigint  NULL,
  [ModifiedDate] datetime  NULL
)
GO

ALTER TABLE [Form].[FormGroup] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单组别Id',
'SCHEMA', N'Form',
'TABLE', N'FormGroup',
'COLUMN', N'FormGroupId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单组别名称（中文）',
'SCHEMA', N'Form',
'TABLE', N'FormGroup',
'COLUMN', N'FormGroupNameCn'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单组别名称（英文）',
'SCHEMA', N'Form',
'TABLE', N'FormGroup',
'COLUMN', N'FormGroupNameEn'
GO

EXEC sp_addextendedproperty
'MS_Description', N'排序',
'SCHEMA', N'Form',
'TABLE', N'FormGroup',
'COLUMN', N'SortOrder'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单组别描述（中文）',
'SCHEMA', N'Form',
'TABLE', N'FormGroup',
'COLUMN', N'DescriptionCn'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单组别描述（英文）',
'SCHEMA', N'Form',
'TABLE', N'FormGroup',
'COLUMN', N'DescriptionEn'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'Form',
'TABLE', N'FormGroup',
'COLUMN', N'CreatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'Form',
'TABLE', N'FormGroup',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改人',
'SCHEMA', N'Form',
'TABLE', N'FormGroup',
'COLUMN', N'ModifiedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改时间',
'SCHEMA', N'Form',
'TABLE', N'FormGroup',
'COLUMN', N'ModifiedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单组别表',
'SCHEMA', N'Form',
'TABLE', N'FormGroup'
GO


-- ----------------------------
-- Records of FormGroup
-- ----------------------------
INSERT INTO [Form].[FormGroup] ([FormGroupId], [FormGroupNameCn], [FormGroupNameEn], [SortOrder], [DescriptionCn], [DescriptionEn], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'1969052085492256768', N'工程类', N'Engineering Category', N'2', N'', NULL, N'1903486709602062336', N'2025-09-25 13:51:23.000', NULL, NULL)
GO

INSERT INTO [Form].[FormGroup] ([FormGroupId], [FormGroupNameCn], [FormGroupNameEn], [SortOrder], [DescriptionCn], [DescriptionEn], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'1969053776929230848', N'管理类', N'Management Category', N'3', N'', NULL, N'1903486709602062336', N'2025-09-25 13:51:23.000', NULL, NULL)
GO

INSERT INTO [Form].[FormGroup] ([FormGroupId], [FormGroupNameCn], [FormGroupNameEn], [SortOrder], [DescriptionCn], [DescriptionEn], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'1969054482025287680', N'总务类', N'General Affairs Category', N'4', N'', NULL, N'1903486709602062336', N'2025-09-25 13:51:23.000', NULL, NULL)
GO

INSERT INTO [Form].[FormGroup] ([FormGroupId], [FormGroupNameCn], [FormGroupNameEn], [SortOrder], [DescriptionCn], [DescriptionEn], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'1969054690842906624', N'业务类', N'Business Category', N'5', N'', NULL, N'1903486709602062336', N'2025-09-25 13:51:23.000', NULL, NULL)
GO

INSERT INTO [Form].[FormGroup] ([FormGroupId], [FormGroupNameCn], [FormGroupNameEn], [SortOrder], [DescriptionCn], [DescriptionEn], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'1969054813085896704', N'采购类', N'Procurement Category', N'6', N'', NULL, N'1903486709602062336', N'2025-09-25 13:51:23.000', NULL, NULL)
GO

INSERT INTO [Form].[FormGroup] ([FormGroupId], [FormGroupNameCn], [FormGroupNameEn], [SortOrder], [DescriptionCn], [DescriptionEn], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'1969055160932110336', N'法务类', N'Legal Affairs Category', N'7', N'', NULL, N'1903486709602062336', N'2025-09-25 13:51:23.000', NULL, NULL)
GO

INSERT INTO [Form].[FormGroup] ([FormGroupId], [FormGroupNameCn], [FormGroupNameEn], [SortOrder], [DescriptionCn], [DescriptionEn], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'1969055351626141696', N'资讯类', N'Information Category', N'8', N'', NULL, N'1903486709602062336', N'2025-09-25 13:51:23.000', NULL, NULL)
GO

INSERT INTO [Form].[FormGroup] ([FormGroupId], [FormGroupNameCn], [FormGroupNameEn], [SortOrder], [DescriptionCn], [DescriptionEn], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'1969055451307970560', N'品保类', N'Quality Assurance Category', N'9', N'', NULL, N'1903486709602062336', N'2025-09-25 13:51:23.000', NULL, NULL)
GO

INSERT INTO [Form].[FormGroup] ([FormGroupId], [FormGroupNameCn], [FormGroupNameEn], [SortOrder], [DescriptionCn], [DescriptionEn], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'1969055549681176576', N'关务类', N'Customs Category', N'10', N'', NULL, N'1903486709602062336', N'2025-09-25 13:51:23.000', NULL, NULL)
GO

INSERT INTO [Form].[FormGroup] ([FormGroupId], [FormGroupNameCn], [FormGroupNameEn], [SortOrder], [DescriptionCn], [DescriptionEn], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'1969055723409248256', N'模具类', N'Mold Category', N'11', N'', NULL, N'1903486709602062336', N'2025-09-25 13:51:23.000', NULL, NULL)
GO

INSERT INTO [Form].[FormGroup] ([FormGroupId], [FormGroupNameCn], [FormGroupNameEn], [SortOrder], [DescriptionCn], [DescriptionEn], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'1969055819815325696', N'财务类', N'Finance Category', N'12', N'', N'', N'1903486709602062336', N'2025-09-25 13:51:23.000', N'1903486709602062336', N'2026-06-28 17:10:39.000')
GO

INSERT INTO [Form].[FormGroup] ([FormGroupId], [FormGroupNameCn], [FormGroupNameEn], [SortOrder], [DescriptionCn], [DescriptionEn], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'1987215338470772736', N'人事类', N'Human Resources', N'1', N'', N'', N'1903486709602062336', N'2025-11-09 01:47:12.000', NULL, NULL)
GO


-- ----------------------------
-- Table structure for FormInstance
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Form].[FormInstance]') AND type IN ('U'))
	DROP TABLE [Form].[FormInstance]
GO

CREATE TABLE [Form].[FormInstance] (
  [FormId] bigint  NOT NULL,
  [FormTypeId] bigint  NOT NULL,
  [FormNo] nvarchar(30) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [FormStatus] nvarchar(30) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [ApplicantUserId] bigint  NOT NULL,
  [ApplicantDate] date  NULL,
  [RuleId] bigint  NULL,
  [CurrentStepId] bigint  NULL,
  [CreatedBy] bigint  NULL,
  [CreatedDate] datetime2(3)  NULL,
  [ModifiedBy] bigint  NULL,
  [ModifiedDate] datetime2(3)  NULL
)
GO

ALTER TABLE [Form].[FormInstance] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单Id',
'SCHEMA', N'Form',
'TABLE', N'FormInstance',
'COLUMN', N'FormId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单类型Id',
'SCHEMA', N'Form',
'TABLE', N'FormInstance',
'COLUMN', N'FormTypeId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单编号',
'SCHEMA', N'Form',
'TABLE', N'FormInstance',
'COLUMN', N'FormNo'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单状态',
'SCHEMA', N'Form',
'TABLE', N'FormInstance',
'COLUMN', N'FormStatus'
GO

EXEC sp_addextendedproperty
'MS_Description', N'申请人Id',
'SCHEMA', N'Form',
'TABLE', N'FormInstance',
'COLUMN', N'ApplicantUserId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'申请时间',
'SCHEMA', N'Form',
'TABLE', N'FormInstance',
'COLUMN', N'ApplicantDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'所属规则Id',
'SCHEMA', N'Form',
'TABLE', N'FormInstance',
'COLUMN', N'RuleId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'当前步骤Id',
'SCHEMA', N'Form',
'TABLE', N'FormInstance',
'COLUMN', N'CurrentStepId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'Form',
'TABLE', N'FormInstance',
'COLUMN', N'CreatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'Form',
'TABLE', N'FormInstance',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改人',
'SCHEMA', N'Form',
'TABLE', N'FormInstance',
'COLUMN', N'ModifiedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改时间',
'SCHEMA', N'Form',
'TABLE', N'FormInstance',
'COLUMN', N'ModifiedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单实例表',
'SCHEMA', N'Form',
'TABLE', N'FormInstance'
GO


-- ----------------------------
-- Records of FormInstance
-- ----------------------------
INSERT INTO [Form].[FormInstance] ([FormId], [FormTypeId], [FormNo], [FormStatus], [ApplicantUserId], [ApplicantDate], [RuleId], [CurrentStepId], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079842855760826368', N'1987217256446300160', N'LVR-2026070001', N'Approved', N'2050602218733834240', N'2026-07-22', N'2079768821333364736', NULL, N'2050602218733834240', N'2026-07-22 16:15:53.083', N'2050596970468347904', N'2026-07-23 11:42:16.220')
GO

INSERT INTO [Form].[FormInstance] ([FormId], [FormTypeId], [FormNo], [FormStatus], [ApplicantUserId], [ApplicantDate], [RuleId], [CurrentStepId], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2080189830318395392', N'2074764225741459456', N'LCF-2026070001', N'PendingSubmit', N'2050602218733834240', N'2026-07-23', N'2079831015152553984', N'2079807667064410112', N'2050602218733834240', N'2026-07-23 15:14:38.263', N'2050602218733834240', N'2026-07-23 16:23:41.597')
GO

INSERT INTO [Form].[FormInstance] ([FormId], [FormTypeId], [FormNo], [FormStatus], [ApplicantUserId], [ApplicantDate], [RuleId], [CurrentStepId], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2080206559140515840', N'1987217256446300160', N'LVR-2026070002', N'Voided', N'2050602218733834240', N'2026-07-23', N'2079768821333364736', N'2009890853346217984', N'2050602218733834240', N'2026-07-23 16:21:06.723', N'2050602218733834240', N'2026-07-23 16:22:48.290')
GO


-- ----------------------------
-- Table structure for FormNotifyToken
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Form].[FormNotifyToken]') AND type IN ('U'))
	DROP TABLE [Form].[FormNotifyToken]
GO

CREATE TABLE [Form].[FormNotifyToken] (
  [FormId] bigint  NOT NULL,
  [ReviewUserId] bigint  NOT NULL,
  [Token] nvarchar(100) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [ExpirationTime] datetime2(3)  NOT NULL,
  [CreatedDate] datetime2(3)  NOT NULL
)
GO

ALTER TABLE [Form].[FormNotifyToken] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单Id',
'SCHEMA', N'Form',
'TABLE', N'FormNotifyToken',
'COLUMN', N'FormId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'待审批人Id',
'SCHEMA', N'Form',
'TABLE', N'FormNotifyToken',
'COLUMN', N'ReviewUserId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'Token',
'SCHEMA', N'Form',
'TABLE', N'FormNotifyToken',
'COLUMN', N'Token'
GO

EXEC sp_addextendedproperty
'MS_Description', N'过期时间',
'SCHEMA', N'Form',
'TABLE', N'FormNotifyToken',
'COLUMN', N'ExpirationTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'Form',
'TABLE', N'FormNotifyToken',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单邮件通知Token表',
'SCHEMA', N'Form',
'TABLE', N'FormNotifyToken'
GO


-- ----------------------------
-- Records of FormNotifyToken
-- ----------------------------

-- ----------------------------
-- Table structure for FormReviewRecord
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Form].[FormReviewRecord]') AND type IN ('U'))
	DROP TABLE [Form].[FormReviewRecord]
GO

CREATE TABLE [Form].[FormReviewRecord] (
  [FormId] bigint  NOT NULL,
  [StepId] bigint  NOT NULL,
  [ReviewResult] nvarchar(30) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [RejectStepId] bigint  NULL,
  [Comment] nvarchar(max) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NULL,
  [ReviewType] nvarchar(30) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [AppointmentType] nvarchar(30) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [OriginalUserId] bigint  NOT NULL,
  [OperationUserId] bigint  NOT NULL,
  [ReviewDateTime] datetime2(3)  NOT NULL,
  [RecordStatus] int  NOT NULL
)
GO

ALTER TABLE [Form].[FormReviewRecord] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单Id',
'SCHEMA', N'Form',
'TABLE', N'FormReviewRecord',
'COLUMN', N'FormId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'步骤Id',
'SCHEMA', N'Form',
'TABLE', N'FormReviewRecord',
'COLUMN', N'StepId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'审批结果（签核、驳回）',
'SCHEMA', N'Form',
'TABLE', N'FormReviewRecord',
'COLUMN', N'ReviewResult'
GO

EXEC sp_addextendedproperty
'MS_Description', N'驳回步骤Id',
'SCHEMA', N'Form',
'TABLE', N'FormReviewRecord',
'COLUMN', N'RejectStepId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'意见',
'SCHEMA', N'Form',
'TABLE', N'FormReviewRecord',
'COLUMN', N'Comment'
GO

EXEC sp_addextendedproperty
'MS_Description', N'审批类型（手动操作、系统自动）',
'SCHEMA', N'Form',
'TABLE', N'FormReviewRecord',
'COLUMN', N'ReviewType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'审批身份（实、兼）',
'SCHEMA', N'Form',
'TABLE', N'FormReviewRecord',
'COLUMN', N'AppointmentType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'原本审批员工Id',
'SCHEMA', N'Form',
'TABLE', N'FormReviewRecord',
'COLUMN', N'OriginalUserId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'实际审批员工Id',
'SCHEMA', N'Form',
'TABLE', N'FormReviewRecord',
'COLUMN', N'OperationUserId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'审批时间',
'SCHEMA', N'Form',
'TABLE', N'FormReviewRecord',
'COLUMN', N'ReviewDateTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'是否有效',
'SCHEMA', N'Form',
'TABLE', N'FormReviewRecord',
'COLUMN', N'RecordStatus'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单审批记录表',
'SCHEMA', N'Form',
'TABLE', N'FormReviewRecord'
GO


-- ----------------------------
-- Records of FormReviewRecord
-- ----------------------------
INSERT INTO [Form].[FormReviewRecord] ([FormId], [StepId], [ReviewResult], [RejectStepId], [Comment], [ReviewType], [AppointmentType], [OriginalUserId], [OperationUserId], [ReviewDateTime], [RecordStatus]) VALUES (N'2079842855760826368', N'2009890853346217984', N'Approve', NULL, N'', N'Manual', N'Actual', N'2050602218733834240', N'2050602218733834240', N'2026-07-22 16:36:35.227', N'1')
GO

INSERT INTO [Form].[FormReviewRecord] ([FormId], [StepId], [ReviewResult], [RejectStepId], [Comment], [ReviewType], [AppointmentType], [OriginalUserId], [OperationUserId], [ReviewDateTime], [RecordStatus]) VALUES (N'2079842855760826368', N'2079394196795559936', N'Approve', NULL, N'', N'Manual', N'AutoActual', N'2050596970468347904', N'2050596970468347904', N'2026-07-23 11:42:16.560', N'1')
GO

INSERT INTO [Form].[FormReviewRecord] ([FormId], [StepId], [ReviewResult], [RejectStepId], [Comment], [ReviewType], [AppointmentType], [OriginalUserId], [OperationUserId], [ReviewDateTime], [RecordStatus]) VALUES (N'2079842855760826368', N'2079394286754992128', N'Approve', NULL, N'', N'Automatic', N'Actual', N'2050596970468347904', N'2050596970468347904', N'2026-07-23 11:42:16.750', N'1')
GO


-- ----------------------------
-- Table structure for FormSequence
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Form].[FormSequence]') AND type IN ('U'))
	DROP TABLE [Form].[FormSequence]
GO

CREATE TABLE [Form].[FormSequence] (
  [FormTypeId] bigint  NOT NULL,
  [Ym] char(6) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [Total] int DEFAULT 0 NOT NULL,
  [CreatedBy] bigint  NOT NULL,
  [CreatedDate] datetime2(3)  NOT NULL,
  [ModifiedBy] bigint  NULL,
  [ModifiedDate] datetime2(3)  NULL
)
GO

ALTER TABLE [Form].[FormSequence] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单类型Id',
'SCHEMA', N'Form',
'TABLE', N'FormSequence',
'COLUMN', N'FormTypeId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'年月（yyyyMM）',
'SCHEMA', N'Form',
'TABLE', N'FormSequence',
'COLUMN', N'Ym'
GO

EXEC sp_addextendedproperty
'MS_Description', N'当前表单数量',
'SCHEMA', N'Form',
'TABLE', N'FormSequence',
'COLUMN', N'Total'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'Form',
'TABLE', N'FormSequence',
'COLUMN', N'CreatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'Form',
'TABLE', N'FormSequence',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改人',
'SCHEMA', N'Form',
'TABLE', N'FormSequence',
'COLUMN', N'ModifiedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改时间',
'SCHEMA', N'Form',
'TABLE', N'FormSequence',
'COLUMN', N'ModifiedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单审计表',
'SCHEMA', N'Form',
'TABLE', N'FormSequence'
GO


-- ----------------------------
-- Records of FormSequence
-- ----------------------------
INSERT INTO [Form].[FormSequence] ([FormTypeId], [Ym], [Total], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'1987217256446300160', N'202607', N'2', N'2050602218733834240', N'2026-07-22 16:15:53.083', N'2050602218733834240', N'2026-07-23 16:21:06.723')
GO

INSERT INTO [Form].[FormSequence] ([FormTypeId], [Ym], [Total], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2074764225741459456', N'202607', N'1', N'2050602218733834240', N'2026-07-23 15:14:38.263', NULL, NULL)
GO


-- ----------------------------
-- Table structure for FormType
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Form].[FormType]') AND type IN ('U'))
	DROP TABLE [Form].[FormType]
GO

CREATE TABLE [Form].[FormType] (
  [FormTypeId] bigint  NOT NULL,
  [FormGroupId] bigint  NOT NULL,
  [FormTypeNameCn] nvarchar(50) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [FormTypeNameEn] nvarchar(70) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [Prefix] nvarchar(5) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [ReviewPath] nvarchar(50) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [ViewPath] nvarchar(50) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [SortOrder] int  NOT NULL,
  [DescriptionCn] nvarchar(300) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NULL,
  [DescriptionEn] nvarchar(500) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NULL,
  [CreatedBy] bigint  NOT NULL,
  [CreatedDate] datetime2(3)  NOT NULL,
  [ModifiedBy] bigint  NULL,
  [ModifiedDate] datetime2(3)  NULL
)
GO

ALTER TABLE [Form].[FormType] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单类型Id',
'SCHEMA', N'Form',
'TABLE', N'FormType',
'COLUMN', N'FormTypeId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'所属表单组别Id',
'SCHEMA', N'Form',
'TABLE', N'FormType',
'COLUMN', N'FormGroupId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单类型名称（中文）',
'SCHEMA', N'Form',
'TABLE', N'FormType',
'COLUMN', N'FormTypeNameCn'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单类型名称（英文）',
'SCHEMA', N'Form',
'TABLE', N'FormType',
'COLUMN', N'FormTypeNameEn'
GO

EXEC sp_addextendedproperty
'MS_Description', N'前缀',
'SCHEMA', N'Form',
'TABLE', N'FormType',
'COLUMN', N'Prefix'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单审批路径',
'SCHEMA', N'Form',
'TABLE', N'FormType',
'COLUMN', N'ReviewPath'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单视图路径',
'SCHEMA', N'Form',
'TABLE', N'FormType',
'COLUMN', N'ViewPath'
GO

EXEC sp_addextendedproperty
'MS_Description', N'排序',
'SCHEMA', N'Form',
'TABLE', N'FormType',
'COLUMN', N'SortOrder'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单类别描述（中文）',
'SCHEMA', N'Form',
'TABLE', N'FormType',
'COLUMN', N'DescriptionCn'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单类别描述（英文）',
'SCHEMA', N'Form',
'TABLE', N'FormType',
'COLUMN', N'DescriptionEn'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'Form',
'TABLE', N'FormType',
'COLUMN', N'CreatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'Form',
'TABLE', N'FormType',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改人',
'SCHEMA', N'Form',
'TABLE', N'FormType',
'COLUMN', N'ModifiedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改时间',
'SCHEMA', N'Form',
'TABLE', N'FormType',
'COLUMN', N'ModifiedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单类别表',
'SCHEMA', N'Form',
'TABLE', N'FormType'
GO


-- ----------------------------
-- Records of FormType
-- ----------------------------
INSERT INTO [Form].[FormType] ([FormTypeId], [FormGroupId], [FormTypeNameCn], [FormTypeNameEn], [Prefix], [ReviewPath], [ViewPath], [SortOrder], [DescriptionCn], [DescriptionEn], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'1987217256446300160', N'1987215338470772736', N'请假单', N'Leave Request Form', N'LVR', N'formbusiness/forms/leaverequest/leaverequest_r', N'formbusiness/forms/leaverequest/leaverequest_v', N'1', N'请假单用于员工因个人事由、病假、事假、年假等原因需要离开工作岗位时，向所属部门及管理层提出请假申请、审批与备案的业务单据。该单据记录请假类型、请假时间、时长、事由以及审批流程，用于确保人员安排合理、流程合规与人事数据准确。', N'A Leave Request Form is used when an employee needs to be absent from work due to personal reasons, sickness, annual leave, or other approved leave types. The form is submitted to the employee’s department and management for approval and record-keeping. It captures the leave type, leave period, duration, reason, and approval workflow, ensuring proper staffing, compliance, and accurate HR records.', N'1903486709602062336', N'2025-11-09 01:54:49.000', N'1903486709602062336', N'2026-06-28 17:06:17.340')
GO

INSERT INTO [Form].[FormType] ([FormTypeId], [FormGroupId], [FormTypeNameCn], [FormTypeNameEn], [Prefix], [ReviewPath], [ViewPath], [SortOrder], [DescriptionCn], [DescriptionEn], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2074764225741459456', N'1987215338470772736', N'销假单', N'Leave Cancellation Form', N'LCF', N'formbusiness/forms/leavecancell/leavecancell_r', N'formbusiness/forms/leavecancell/leavecancell_v', N'2', N'销假单用于员工在请假结束、提前返岗或需要取消/终止原请假记录时提交申请。该单据通常关联原请假单，用于确认员工实际返岗时间，并更新请假记录及考勤状态。', N'Leave Cancellation Form is used when an employee ends a leave, returns to work early, or needs to cancel/terminate an existing leave request. This form is usually linked to the original leave request form and is used to confirm the employee’s actual return date and update the leave and attendance records.', N'1903486709602062336', N'2026-07-08 15:55:13.330', NULL, NULL)
GO


-- ----------------------------
-- Table structure for FormTypeField
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Form].[FormTypeField]') AND type IN ('U'))
	DROP TABLE [Form].[FormTypeField]
GO

CREATE TABLE [Form].[FormTypeField] (
  [FieldId] bigint  NOT NULL,
  [FormTypeId] bigint  NOT NULL,
  [FieldKey] nvarchar(20) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [FieldNameCn] nvarchar(15) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [FieldNameEn] nvarchar(30) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [SortOrder] int  NULL,
  [CreatedBy] bigint  NULL,
  [CreatedDate] datetime2(3)  NULL,
  [ModifiedBy] bigint  NULL,
  [ModifiedDate] datetime2(3)  NULL
)
GO

ALTER TABLE [Form].[FormTypeField] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'栏位Id',
'SCHEMA', N'Form',
'TABLE', N'FormTypeField',
'COLUMN', N'FieldId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单类型Id',
'SCHEMA', N'Form',
'TABLE', N'FormTypeField',
'COLUMN', N'FormTypeId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'栏位Key',
'SCHEMA', N'Form',
'TABLE', N'FormTypeField',
'COLUMN', N'FieldKey'
GO

EXEC sp_addextendedproperty
'MS_Description', N'栏位名称（中文）',
'SCHEMA', N'Form',
'TABLE', N'FormTypeField',
'COLUMN', N'FieldNameCn'
GO

EXEC sp_addextendedproperty
'MS_Description', N'栏位名称（英文）',
'SCHEMA', N'Form',
'TABLE', N'FormTypeField',
'COLUMN', N'FieldNameEn'
GO

EXEC sp_addextendedproperty
'MS_Description', N'排序',
'SCHEMA', N'Form',
'TABLE', N'FormTypeField',
'COLUMN', N'SortOrder'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'Form',
'TABLE', N'FormTypeField',
'COLUMN', N'CreatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'Form',
'TABLE', N'FormTypeField',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改人',
'SCHEMA', N'Form',
'TABLE', N'FormTypeField',
'COLUMN', N'ModifiedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改时间',
'SCHEMA', N'Form',
'TABLE', N'FormTypeField',
'COLUMN', N'ModifiedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单步骤栏位权限表',
'SCHEMA', N'Form',
'TABLE', N'FormTypeField'
GO


-- ----------------------------
-- Records of FormTypeField
-- ----------------------------
INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2059813328053735424', N'1987217256446300160', N'FormNo', N'表单编号', N'Form No', N'1', N'1903486709602062336', N'2026-05-28 09:45:41.547', N'1903486709602062336', N'2026-05-28 09:46:19.583')
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2059839522363019264', N'1987217256446300160', N'UserNo', N'申请人工号', N'User No', N'3', N'1903486709602062336', N'2026-05-28 11:29:46.757', N'1903486709602062336', N'2026-05-31 02:05:38.670')
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2059839903075799041', N'1987217256446300160', N'Department', N'申请人部门', N'Department', N'5', N'1903486709602062336', N'2026-05-28 11:28:57.837', N'1903486709602062336', N'2026-06-14 20:36:57.587')
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2060784098187808768', N'1987217256446300160', N'LeaveType', N'请假类型', N'Leave Type', N'7', N'1903486709602062336', N'2026-05-31 02:03:11.183', N'1903486709602062336', N'2026-06-14 20:37:13.073')
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2060784667472302080', N'1987217256446300160', N'LeavePeriod', N'请假时间段', N'Leave Period', N'8', N'1903486709602062336', N'2026-05-31 02:05:26.910', N'1903486709602062336', N'2026-06-14 20:37:19.400')
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2060785386585722880', N'1987217256446300160', N'SelectAgent', N'代理人选择', N'Select Agent', N'9', N'1903486709602062336', N'2026-05-31 02:08:18.360', N'1903486709602062336', N'2026-07-22 13:34:45.843')
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2060785728597659648', N'1987217256446300160', N'LeaveHours', N'请假时数', N'Leave Hours', N'10', N'1903486709602062336', N'2026-05-31 02:09:39.903', N'1903486709602062336', N'2026-06-14 20:37:25.070')
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2060785864845430784', N'1987217256446300160', N'LeaveReason', N'请假事由', N'Reason for Leave', N'11', N'1903486709602062336', N'2026-05-31 02:10:12.387', N'1903486709602062336', N'2026-06-14 20:37:28.293')
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2060786020718350336', N'1987217256446300160', N'Upload', N'上传附件', N'Upload attachment', N'12', N'1903486709602062336', N'2026-05-31 02:10:49.550', N'1903486709602062336', N'2026-06-14 20:37:32.210')
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2060786240139169792', N'1987217256446300160', N'Save', N'保存', N'Save', N'14', N'1903486709602062336', N'2026-05-31 02:11:41.863', N'1903486709602062336', N'2026-07-23 16:28:28.713')
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2060786334641033216', N'1987217256446300160', N'Submit', N'送审', N'Submit', N'15', N'1903486709602062336', N'2026-05-31 02:12:04.393', N'1903486709602062336', N'2026-07-23 16:28:34.473')
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2060786455856418816', N'1987217256446300160', N'Reject', N'驳回', N'Reject', N'16', N'1903486709602062336', N'2026-05-31 02:12:33.293', N'1903486709602062336', N'2026-07-23 16:28:37.163')
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2066137776289615872', N'1987217256446300160', N'ApplyDate', N'申请日期', N'Apply Date', N'2', N'1903486709602062336', N'2026-06-14 20:36:47.510', N'1903486709602062336', N'2026-06-14 20:38:32.107')
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2066887051650928640', N'1987217256446300160', N'LeaveBalance', N'假期余额', N'Leave Balance', N'6', N'1903486709602062336', N'2026-06-16 22:14:08.677', NULL, NULL)
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079802606770851840', N'2074764225741459456', N'FormNo', N'表单编号', N'Form No', N'1', N'1903486709602062336', N'2026-07-22 13:35:56.977', NULL, NULL)
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079802708130402304', N'2074764225741459456', N'ApplyDate', N'申请日期', N'Apply Date', N'2', N'1903486709602062336', N'2026-07-22 13:36:21.143', NULL, NULL)
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079802831254196224', N'2074764225741459456', N'UserNo', N'表单编号', N'User No', N'3', N'1903486709602062336', N'2026-07-22 13:36:50.497', NULL, NULL)
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079802896626618368', N'1987217256446300160', N'UserName', N'申请人姓名', N'User Name', N'4', N'1903486709602062336', N'2026-07-22 13:37:06.083', N'1903486709602062336', N'2026-07-22 13:37:33.537')
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079802976205148160', N'2074764225741459456', N'UserName', N'申请人姓名', N'User Name', N'4', N'1903486709602062336', N'2026-07-22 13:37:25.057', N'1903486709602062336', N'2026-07-22 13:37:44.783')
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079803153322217472', N'2074764225741459456', N'Department', N'申请人部门', N'Department', N'5', N'1903486709602062336', N'2026-07-22 13:38:07.283', NULL, NULL)
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079803460731146240', N'2074764225741459456', N'SelectLeaveRequest', N'选择请假单', N'Select LeaveRequest', N'6', N'1903486709602062336', N'2026-07-22 13:39:20.577', NULL, NULL)
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079803673143283712', N'2074764225741459456', N'AvailableHours', N'可销时数', N'Available Hours', N'7', N'1903486709602062336', N'2026-07-22 13:40:11.220', N'1903486709602062336', N'2026-07-23 16:44:07.383')
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079804002605862912', N'2074764225741459456', N'TimePeriod', N'时间段', N'Time Period', N'8', N'1903486709602062336', N'2026-07-22 13:41:29.770', NULL, NULL)
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807502907740160', N'2074764225741459456', N'Hour', N'时数', N'Hour', N'9', N'1903486709602062336', N'2026-07-22 13:55:24.307', N'1903486709602062336', N'2026-07-22 13:55:28.287')
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2080207951758495744', N'2074764225741459456', N'Comments', N'送审意见', N'Comments', N'10', N'1903486709602062336', N'2026-07-23 16:26:38.753', NULL, NULL)
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2080208102950572032', N'2074764225741459456', N'Save', N'保存', N'Save', N'11', N'1903486709602062336', N'2026-07-23 16:27:14.800', NULL, NULL)
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2080208155173851136', N'2074764225741459456', N'Submit', N'送审', N'Submit', N'12', N'1903486709602062336', N'2026-07-23 16:27:27.250', NULL, NULL)
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2080208205656494080', N'2074764225741459456', N'Reject', N'驳回', N'Reject', N'13', N'1903486709602062336', N'2026-07-23 16:27:39.287', NULL, NULL)
GO

INSERT INTO [Form].[FormTypeField] ([FieldId], [FormTypeId], [FieldKey], [FieldNameCn], [FieldNameEn], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2080208377132224512', N'1987217256446300160', N'Comments', N'送审意见', N'Comments', N'13', N'1903486709602062336', N'2026-07-23 16:28:20.167', NULL, NULL)
GO


-- ----------------------------
-- Table structure for LeaveCancell
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Form].[LeaveCancell]') AND type IN ('U'))
	DROP TABLE [Form].[LeaveCancell]
GO

CREATE TABLE [Form].[LeaveCancell] (
  [FormId] bigint  NOT NULL,
  [LeaveRequestId] bigint  NULL,
  [LeaveRequestNo] nvarchar(30) COLLATE Chinese_PRC_CI_AS  NULL,
  [StartDateTime] datetime2(7)  NULL,
  [EndDateTime] datetime2(7)  NULL,
  [CancellHours] decimal(6,2)  NULL,
  [CreatedBy] bigint  NOT NULL,
  [CreatedDate] datetime2(3)  NOT NULL,
  [ModifiedBy] bigint  NULL,
  [ModifiedDate] datetime2(3)  NULL
)
GO

ALTER TABLE [Form].[LeaveCancell] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'销假单Id',
'SCHEMA', N'Form',
'TABLE', N'LeaveCancell',
'COLUMN', N'FormId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'请假单Id',
'SCHEMA', N'Form',
'TABLE', N'LeaveCancell',
'COLUMN', N'LeaveRequestId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'请假单号',
'SCHEMA', N'Form',
'TABLE', N'LeaveCancell',
'COLUMN', N'LeaveRequestNo'
GO

EXEC sp_addextendedproperty
'MS_Description', N'销假开始时间',
'SCHEMA', N'Form',
'TABLE', N'LeaveCancell',
'COLUMN', N'StartDateTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'销假结束时间',
'SCHEMA', N'Form',
'TABLE', N'LeaveCancell',
'COLUMN', N'EndDateTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'销假时数',
'SCHEMA', N'Form',
'TABLE', N'LeaveCancell',
'COLUMN', N'CancellHours'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'Form',
'TABLE', N'LeaveCancell',
'COLUMN', N'CreatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'Form',
'TABLE', N'LeaveCancell',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改人',
'SCHEMA', N'Form',
'TABLE', N'LeaveCancell',
'COLUMN', N'ModifiedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改时间',
'SCHEMA', N'Form',
'TABLE', N'LeaveCancell',
'COLUMN', N'ModifiedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'销假单表',
'SCHEMA', N'Form',
'TABLE', N'LeaveCancell'
GO


-- ----------------------------
-- Records of LeaveCancell
-- ----------------------------
INSERT INTO [Form].[LeaveCancell] ([FormId], [LeaveRequestId], [LeaveRequestNo], [StartDateTime], [EndDateTime], [CancellHours], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2080189830318395392', N'2079842855760826368', N'LVR-2026070001', N'2026-07-23 08:00:00.0000000', N'2026-07-24 17:00:00.0000000', N'16.00', N'2050602218733834240', N'2026-07-23 15:14:38.323', N'2050602218733834240', N'2026-07-23 16:23:41.553')
GO


-- ----------------------------
-- Table structure for LeaveRequest
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Form].[LeaveRequest]') AND type IN ('U'))
	DROP TABLE [Form].[LeaveRequest]
GO

CREATE TABLE [Form].[LeaveRequest] (
  [FormId] bigint  NOT NULL,
  [LeaveType] nvarchar(30) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NULL,
  [LeaveReason] nvarchar(150) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NULL,
  [StartDateTime] datetime2(7)  NULL,
  [EndDateTime] datetime2(7)  NULL,
  [LeaveHours] decimal(6,2)  NOT NULL,
  [AgentUserId] bigint  NULL,
  [AgentUserName] nvarchar(30) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NULL,
  [CreatedBy] bigint  NULL,
  [CreatedDate] datetime2(3)  NULL,
  [ModifiedBy] bigint  NULL,
  [ModifiedDate] datetime2(3)  NULL
)
GO

ALTER TABLE [Form].[LeaveRequest] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'请假单Id',
'SCHEMA', N'Form',
'TABLE', N'LeaveRequest',
'COLUMN', N'FormId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'请假假别',
'SCHEMA', N'Form',
'TABLE', N'LeaveRequest',
'COLUMN', N'LeaveType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'请假事由',
'SCHEMA', N'Form',
'TABLE', N'LeaveRequest',
'COLUMN', N'LeaveReason'
GO

EXEC sp_addextendedproperty
'MS_Description', N'请假开始时间',
'SCHEMA', N'Form',
'TABLE', N'LeaveRequest',
'COLUMN', N'StartDateTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'请假结束时间',
'SCHEMA', N'Form',
'TABLE', N'LeaveRequest',
'COLUMN', N'EndDateTime'
GO

EXEC sp_addextendedproperty
'MS_Description', N'请假时数',
'SCHEMA', N'Form',
'TABLE', N'LeaveRequest',
'COLUMN', N'LeaveHours'
GO

EXEC sp_addextendedproperty
'MS_Description', N'代理人Id',
'SCHEMA', N'Form',
'TABLE', N'LeaveRequest',
'COLUMN', N'AgentUserId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'代理人姓名',
'SCHEMA', N'Form',
'TABLE', N'LeaveRequest',
'COLUMN', N'AgentUserName'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'Form',
'TABLE', N'LeaveRequest',
'COLUMN', N'CreatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'Form',
'TABLE', N'LeaveRequest',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改人',
'SCHEMA', N'Form',
'TABLE', N'LeaveRequest',
'COLUMN', N'ModifiedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改时间',
'SCHEMA', N'Form',
'TABLE', N'LeaveRequest',
'COLUMN', N'ModifiedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'请假单表',
'SCHEMA', N'Form',
'TABLE', N'LeaveRequest'
GO


-- ----------------------------
-- Records of LeaveRequest
-- ----------------------------
INSERT INTO [Form].[LeaveRequest] ([FormId], [LeaveType], [LeaveReason], [StartDateTime], [EndDateTime], [LeaveHours], [AgentUserId], [AgentUserName], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079773283061993472', NULL, NULL, NULL, NULL, N'0.00', NULL, NULL, N'2050602218733834240', N'2026-07-22 11:39:25.757', NULL, NULL)
GO

INSERT INTO [Form].[LeaveRequest] ([FormId], [LeaveType], [LeaveReason], [StartDateTime], [EndDateTime], [LeaveHours], [AgentUserId], [AgentUserName], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079842855760826368', N'Personal', N'家中有事。', N'2026-07-23 08:00:00.0000000', N'2026-07-24 17:00:00.0000000', N'16.00', N'2050601504599052288', N'E350428 / 李静远', N'2050602218733834240', N'2026-07-22 16:15:53.130', N'2050596970468347904', N'2026-07-23 11:42:16.063')
GO

INSERT INTO [Form].[LeaveRequest] ([FormId], [LeaveType], [LeaveReason], [StartDateTime], [EndDateTime], [LeaveHours], [AgentUserId], [AgentUserName], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2080206559140515840', NULL, NULL, NULL, NULL, N'0.00', NULL, NULL, N'2050602218733834240', N'2026-07-23 16:21:06.860', NULL, NULL)
GO


-- ----------------------------
-- Table structure for PendingReview
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Form].[PendingReview]') AND type IN ('U'))
	DROP TABLE [Form].[PendingReview]
GO

CREATE TABLE [Form].[PendingReview] (
  [FormId] bigint  NOT NULL,
  [StepId] bigint  NULL,
  [AppointmentType] nvarchar(15) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NULL,
  [ReviewUserId] bigint  NULL
)
GO

ALTER TABLE [Form].[PendingReview] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单Id',
'SCHEMA', N'Form',
'TABLE', N'PendingReview',
'COLUMN', N'FormId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'步骤Id',
'SCHEMA', N'Form',
'TABLE', N'PendingReview',
'COLUMN', N'StepId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'待审批人身份（实、兼）',
'SCHEMA', N'Form',
'TABLE', N'PendingReview',
'COLUMN', N'AppointmentType'
GO

EXEC sp_addextendedproperty
'MS_Description', N'待审批人Id',
'SCHEMA', N'Form',
'TABLE', N'PendingReview',
'COLUMN', N'ReviewUserId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单待签核人员表',
'SCHEMA', N'Form',
'TABLE', N'PendingReview'
GO


-- ----------------------------
-- Records of PendingReview
-- ----------------------------
INSERT INTO [Form].[PendingReview] ([FormId], [StepId], [AppointmentType], [ReviewUserId]) VALUES (N'2080189830318395392', N'2079807667064410112', N'Actual', N'2050602218733834240')
GO

INSERT INTO [Form].[PendingReview] ([FormId], [StepId], [AppointmentType], [ReviewUserId]) VALUES (N'2080206559140515840', N'2009890853346217984', N'Actual', N'2050602218733834240')
GO


-- ----------------------------
-- Table structure for StepFieldPermission
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Form].[StepFieldPermission]') AND type IN ('U'))
	DROP TABLE [Form].[StepFieldPermission]
GO

CREATE TABLE [Form].[StepFieldPermission] (
  [StepId] bigint  NOT NULL,
  [FieldId] bigint  NOT NULL,
  [IsVisible] int  NOT NULL,
  [IsDisabled] int  NOT NULL,
  [CreatedBy] bigint  NOT NULL,
  [CreatedDate] datetime2(3)  NOT NULL,
  [ModifiedBy] bigint  NULL,
  [ModifiedDate] datetime2(3)  NULL
)
GO

ALTER TABLE [Form].[StepFieldPermission] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'步骤Id',
'SCHEMA', N'Form',
'TABLE', N'StepFieldPermission',
'COLUMN', N'StepId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'栏位Id',
'SCHEMA', N'Form',
'TABLE', N'StepFieldPermission',
'COLUMN', N'FieldId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'是否显示',
'SCHEMA', N'Form',
'TABLE', N'StepFieldPermission',
'COLUMN', N'IsVisible'
GO

EXEC sp_addextendedproperty
'MS_Description', N'是否禁用',
'SCHEMA', N'Form',
'TABLE', N'StepFieldPermission',
'COLUMN', N'IsDisabled'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'Form',
'TABLE', N'StepFieldPermission',
'COLUMN', N'CreatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'Form',
'TABLE', N'StepFieldPermission',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改人',
'SCHEMA', N'Form',
'TABLE', N'StepFieldPermission',
'COLUMN', N'ModifiedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改时间',
'SCHEMA', N'Form',
'TABLE', N'StepFieldPermission',
'COLUMN', N'ModifiedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单步骤栏位权限表',
'SCHEMA', N'Form',
'TABLE', N'StepFieldPermission'
GO


-- ----------------------------
-- Records of StepFieldPermission
-- ----------------------------
INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009890853346217984', N'2059813328053735424', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:05.353', N'1903486709602062336', N'2026-07-23 16:29:05.353')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009890853346217984', N'2066137776289615872', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:05.353', N'1903486709602062336', N'2026-07-23 16:29:05.353')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009890853346217984', N'2059839522363019264', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:05.353', N'1903486709602062336', N'2026-07-23 16:29:05.353')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009890853346217984', N'2079802896626618368', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:29:05.353', N'1903486709602062336', N'2026-07-23 16:29:05.353')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009890853346217984', N'2059839903075799041', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:05.353', N'1903486709602062336', N'2026-07-23 16:29:05.353')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009890853346217984', N'2066887051650928640', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:05.353', N'1903486709602062336', N'2026-07-23 16:29:05.353')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009890853346217984', N'2060784098187808768', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:29:05.353', N'1903486709602062336', N'2026-07-23 16:29:05.353')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009890853346217984', N'2060784667472302080', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:29:05.353', N'1903486709602062336', N'2026-07-23 16:29:05.353')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009890853346217984', N'2060785386585722880', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:29:05.353', N'1903486709602062336', N'2026-07-23 16:29:05.353')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009890853346217984', N'2060785728597659648', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:05.353', N'1903486709602062336', N'2026-07-23 16:29:05.353')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009890853346217984', N'2060785864845430784', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:29:05.353', N'1903486709602062336', N'2026-07-23 16:29:05.353')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009890853346217984', N'2060786020718350336', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:29:05.353', N'1903486709602062336', N'2026-07-23 16:29:05.353')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009890853346217984', N'2080208377132224512', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:29:05.353', N'1903486709602062336', N'2026-07-23 16:29:05.353')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009890853346217984', N'2060786240139169792', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:29:05.353', N'1903486709602062336', N'2026-07-23 16:29:05.353')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009897830268932096', N'2059813328053735424', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:35.345', N'1903486709602062336', N'2026-07-23 16:29:35.345')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009897830268932096', N'2066137776289615872', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:35.345', N'1903486709602062336', N'2026-07-23 16:29:35.345')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009897830268932096', N'2059839522363019264', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:35.345', N'1903486709602062336', N'2026-07-23 16:29:35.345')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009897830268932096', N'2079802896626618368', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:29:35.345', N'1903486709602062336', N'2026-07-23 16:29:35.345')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009897830268932096', N'2059839903075799041', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:35.345', N'1903486709602062336', N'2026-07-23 16:29:35.345')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009897830268932096', N'2066887051650928640', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:35.345', N'1903486709602062336', N'2026-07-23 16:29:35.345')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009897830268932096', N'2060784098187808768', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:35.345', N'1903486709602062336', N'2026-07-23 16:29:35.345')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009897830268932096', N'2060784667472302080', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:35.345', N'1903486709602062336', N'2026-07-23 16:29:35.345')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009897830268932096', N'2060785386585722880', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:29:35.345', N'1903486709602062336', N'2026-07-23 16:29:35.345')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009897830268932096', N'2060785728597659648', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:35.345', N'1903486709602062336', N'2026-07-23 16:29:35.345')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009892923604340736', N'2059813328053735424', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:14.914', N'1903486709602062336', N'2026-07-23 16:29:14.914')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009897830268932096', N'2060785864845430784', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:35.345', N'1903486709602062336', N'2026-07-23 16:29:35.345')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009897830268932096', N'2060786020718350336', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:35.345', N'1903486709602062336', N'2026-07-23 16:29:35.345')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009897830268932096', N'2080208377132224512', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:29:35.345', N'1903486709602062336', N'2026-07-23 16:29:35.345')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009892923604340736', N'2066137776289615872', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:14.914', N'1903486709602062336', N'2026-07-23 16:29:14.914')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009892923604340736', N'2059839522363019264', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:14.914', N'1903486709602062336', N'2026-07-23 16:29:14.914')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009892923604340736', N'2079802896626618368', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:29:14.914', N'1903486709602062336', N'2026-07-23 16:29:14.914')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009892923604340736', N'2059839903075799041', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:14.914', N'1903486709602062336', N'2026-07-23 16:29:14.914')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009892923604340736', N'2066887051650928640', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:29:14.914', N'1903486709602062336', N'2026-07-23 16:29:14.914')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009892923604340736', N'2060784098187808768', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:14.914', N'1903486709602062336', N'2026-07-23 16:29:14.914')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009892923604340736', N'2060784667472302080', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:14.914', N'1903486709602062336', N'2026-07-23 16:29:14.914')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009892923604340736', N'2060785386585722880', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:29:14.914', N'1903486709602062336', N'2026-07-23 16:29:14.914')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009892923604340736', N'2060785728597659648', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:14.914', N'1903486709602062336', N'2026-07-23 16:29:14.914')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009892923604340736', N'2060785864845430784', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:14.914', N'1903486709602062336', N'2026-07-23 16:29:14.914')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009892923604340736', N'2060786020718350336', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:14.914', N'1903486709602062336', N'2026-07-23 16:29:14.914')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009892923604340736', N'2080208377132224512', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:29:14.914', N'1903486709602062336', N'2026-07-23 16:29:14.914')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009892923604340736', N'2060786240139169792', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:29:14.914', N'1903486709602062336', N'2026-07-23 16:29:14.914')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009898117243211776', N'2059813328053735424', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:58.682', N'1903486709602062336', N'2026-07-23 16:29:58.682')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2036076248547069952', N'2059813328053735424', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:12.869', N'1903486709602062336', N'2026-07-23 16:30:12.869')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2036076248547069952', N'2066137776289615872', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:12.869', N'1903486709602062336', N'2026-07-23 16:30:12.869')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2036076248547069952', N'2059839522363019264', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:12.869', N'1903486709602062336', N'2026-07-23 16:30:12.869')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2036076248547069952', N'2079802896626618368', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:30:12.869', N'1903486709602062336', N'2026-07-23 16:30:12.869')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2036076248547069952', N'2059839903075799041', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:12.869', N'1903486709602062336', N'2026-07-23 16:30:12.869')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2036076248547069952', N'2066887051650928640', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:30:12.869', N'1903486709602062336', N'2026-07-23 16:30:12.869')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2036076248547069952', N'2060784098187808768', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:12.869', N'1903486709602062336', N'2026-07-23 16:30:12.869')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2036076248547069952', N'2060784667472302080', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:12.869', N'1903486709602062336', N'2026-07-23 16:30:12.869')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2036076248547069952', N'2060785386585722880', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:12.869', N'1903486709602062336', N'2026-07-23 16:30:12.869')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2036076248547069952', N'2060785728597659648', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:12.869', N'1903486709602062336', N'2026-07-23 16:30:12.869')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009898117243211776', N'2066137776289615872', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:58.682', N'1903486709602062336', N'2026-07-23 16:29:58.682')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2036076248547069952', N'2060785864845430784', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:12.869', N'1903486709602062336', N'2026-07-23 16:30:12.869')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2036076248547069952', N'2060786020718350336', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:12.869', N'1903486709602062336', N'2026-07-23 16:30:12.869')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2036076248547069952', N'2080208377132224512', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:12.869', N'1903486709602062336', N'2026-07-23 16:30:12.869')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009898117243211776', N'2059839522363019264', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:58.682', N'1903486709602062336', N'2026-07-23 16:29:58.682')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009898117243211776', N'2079802896626618368', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:29:58.682', N'1903486709602062336', N'2026-07-23 16:29:58.682')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009898117243211776', N'2059839903075799041', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:58.682', N'1903486709602062336', N'2026-07-23 16:29:58.682')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009898117243211776', N'2066887051650928640', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:29:58.682', N'1903486709602062336', N'2026-07-23 16:29:58.682')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009898117243211776', N'2060784098187808768', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:58.682', N'1903486709602062336', N'2026-07-23 16:29:58.682')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009898117243211776', N'2060784667472302080', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:58.682', N'1903486709602062336', N'2026-07-23 16:29:58.682')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009898117243211776', N'2060785386585722880', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:29:58.682', N'1903486709602062336', N'2026-07-23 16:29:58.682')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009898117243211776', N'2060785728597659648', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:58.682', N'1903486709602062336', N'2026-07-23 16:29:58.682')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009898117243211776', N'2060785864845430784', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:58.682', N'1903486709602062336', N'2026-07-23 16:29:58.682')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009898117243211776', N'2060786020718350336', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:29:58.682', N'1903486709602062336', N'2026-07-23 16:29:58.682')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009898117243211776', N'2080208377132224512', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:29:58.682', N'1903486709602062336', N'2026-07-23 16:29:58.682')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009898117243211776', N'2060786240139169792', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:29:58.682', N'1903486709602062336', N'2026-07-23 16:29:58.682')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2029389483455156224', N'2059813328053735424', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:07.469', N'1903486709602062336', N'2026-07-23 16:30:07.469')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2029389483455156224', N'2066137776289615872', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:07.469', N'1903486709602062336', N'2026-07-23 16:30:07.469')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010240', N'2059813328053735424', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:35.087', N'1903486709602062336', N'2026-07-23 16:30:35.087')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010240', N'2066137776289615872', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:35.087', N'1903486709602062336', N'2026-07-23 16:30:35.087')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010240', N'2059839522363019264', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:35.087', N'1903486709602062336', N'2026-07-23 16:30:35.087')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010240', N'2079802896626618368', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:30:35.087', N'1903486709602062336', N'2026-07-23 16:30:35.087')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010240', N'2059839903075799041', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:35.087', N'1903486709602062336', N'2026-07-23 16:30:35.087')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010240', N'2066887051650928640', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:35.087', N'1903486709602062336', N'2026-07-23 16:30:35.087')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010240', N'2060784098187808768', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:35.087', N'1903486709602062336', N'2026-07-23 16:30:35.087')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010240', N'2060784667472302080', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:35.087', N'1903486709602062336', N'2026-07-23 16:30:35.087')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010240', N'2060785386585722880', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:35.087', N'1903486709602062336', N'2026-07-23 16:30:35.087')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010240', N'2060785728597659648', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:35.087', N'1903486709602062336', N'2026-07-23 16:30:35.087')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2029389483455156224', N'2059839522363019264', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:07.469', N'1903486709602062336', N'2026-07-23 16:30:07.469')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010240', N'2060785864845430784', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:35.087', N'1903486709602062336', N'2026-07-23 16:30:35.087')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010240', N'2060786020718350336', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:35.087', N'1903486709602062336', N'2026-07-23 16:30:35.087')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010240', N'2080208377132224512', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:35.087', N'1903486709602062336', N'2026-07-23 16:30:35.087')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010286', N'2059813328053735424', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:41.197', N'1903486709602062336', N'2026-07-23 16:30:41.197')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010286', N'2066137776289615872', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:41.197', N'1903486709602062336', N'2026-07-23 16:30:41.197')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010286', N'2059839522363019264', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:41.197', N'1903486709602062336', N'2026-07-23 16:30:41.197')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010286', N'2079802896626618368', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:30:41.197', N'1903486709602062336', N'2026-07-23 16:30:41.197')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010286', N'2059839903075799041', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:41.197', N'1903486709602062336', N'2026-07-23 16:30:41.197')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010286', N'2066887051650928640', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:41.197', N'1903486709602062336', N'2026-07-23 16:30:41.197')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010286', N'2060784098187808768', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:41.197', N'1903486709602062336', N'2026-07-23 16:30:41.197')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010286', N'2060784667472302080', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:41.197', N'1903486709602062336', N'2026-07-23 16:30:41.197')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010286', N'2060785386585722880', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:41.197', N'1903486709602062336', N'2026-07-23 16:30:41.197')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010286', N'2060785728597659648', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:41.197', N'1903486709602062336', N'2026-07-23 16:30:41.197')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2029389483455156224', N'2079802896626618368', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:30:07.469', N'1903486709602062336', N'2026-07-23 16:30:07.469')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010286', N'2060785864845430784', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:41.197', N'1903486709602062336', N'2026-07-23 16:30:41.197')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010286', N'2060786020718350336', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:41.197', N'1903486709602062336', N'2026-07-23 16:30:41.197')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010286', N'2080208377132224512', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:41.197', N'1903486709602062336', N'2026-07-23 16:30:41.197')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2029389483455156224', N'2059839903075799041', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:07.469', N'1903486709602062336', N'2026-07-23 16:30:07.469')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2029389483455156224', N'2066887051650928640', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:30:07.469', N'1903486709602062336', N'2026-07-23 16:30:07.469')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2029389483455156224', N'2060784098187808768', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:07.469', N'1903486709602062336', N'2026-07-23 16:30:07.469')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2029389483455156224', N'2060784667472302080', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:07.469', N'1903486709602062336', N'2026-07-23 16:30:07.469')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2029389483455156224', N'2060785386585722880', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:07.469', N'1903486709602062336', N'2026-07-23 16:30:07.469')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2029389483455156224', N'2060785728597659648', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:07.469', N'1903486709602062336', N'2026-07-23 16:30:07.469')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2029389483455156224', N'2060785864845430784', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:07.469', N'1903486709602062336', N'2026-07-23 16:30:07.469')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2029389483455156224', N'2060786020718350336', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:07.469', N'1903486709602062336', N'2026-07-23 16:30:07.469')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2029389483455156224', N'2080208377132224512', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:07.469', N'1903486709602062336', N'2026-07-23 16:30:07.469')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2029389483455156224', N'2060786240139169792', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:30:07.469', N'1903486709602062336', N'2026-07-23 16:30:07.469')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2036076248547069952', N'2060786240139169792', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:30:12.869', N'1903486709602062336', N'2026-07-23 16:30:12.869')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010240', N'2060786240139169792', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:30:35.087', N'1903486709602062336', N'2026-07-23 16:30:35.087')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010286', N'2060786240139169792', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:30:41.197', N'1903486709602062336', N'2026-07-23 16:30:41.197')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009890853346217984', N'2060786334641033216', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:29:05.353', N'1903486709602062336', N'2026-07-23 16:29:05.353')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009892923604340736', N'2060786334641033216', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:29:14.914', N'1903486709602062336', N'2026-07-23 16:29:14.914')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009897830268932096', N'2060786240139169792', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:29:35.345', N'1903486709602062336', N'2026-07-23 16:29:35.345')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009897830268932096', N'2060786334641033216', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:29:35.345', N'1903486709602062336', N'2026-07-23 16:29:35.345')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009898117243211776', N'2060786334641033216', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:29:58.682', N'1903486709602062336', N'2026-07-23 16:29:58.682')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2029389483455156224', N'2060786334641033216', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:07.469', N'1903486709602062336', N'2026-07-23 16:30:07.469')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2036076248547069952', N'2060786334641033216', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:12.869', N'1903486709602062336', N'2026-07-23 16:30:12.869')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010240', N'2060786334641033216', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:35.087', N'1903486709602062336', N'2026-07-23 16:30:35.087')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010286', N'2060786334641033216', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:41.197', N'1903486709602062336', N'2026-07-23 16:30:41.197')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079393781261668352', N'2059813328053735424', N'1', N'1', N'1903486709602062336', N'2026-07-22 08:56:34.987', N'1903486709602062336', N'2026-07-22 08:56:34.988')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079393781261668352', N'2066137776289615872', N'1', N'1', N'1903486709602062336', N'2026-07-22 08:56:34.988', N'1903486709602062336', N'2026-07-22 08:56:34.988')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079393781261668352', N'2059839522363019264', N'1', N'1', N'1903486709602062336', N'2026-07-22 08:56:34.988', N'1903486709602062336', N'2026-07-22 08:56:34.988')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079393781261668352', N'2059839903075799040', N'1', N'1', N'1903486709602062336', N'2026-07-22 08:56:34.988', N'1903486709602062336', N'2026-07-22 08:56:34.988')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079393781261668352', N'2059839903075799041', N'1', N'1', N'1903486709602062336', N'2026-07-22 08:56:34.988', N'1903486709602062336', N'2026-07-22 08:56:34.988')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079393781261668352', N'2066887051650928640', N'1', N'1', N'1903486709602062336', N'2026-07-22 08:56:34.988', N'1903486709602062336', N'2026-07-22 08:56:34.988')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079393781261668352', N'2060784098187808768', N'1', N'1', N'1903486709602062336', N'2026-07-22 08:56:34.988', N'1903486709602062336', N'2026-07-22 08:56:34.988')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079393781261668352', N'2060784667472302080', N'1', N'1', N'1903486709602062336', N'2026-07-22 08:56:34.988', N'1903486709602062336', N'2026-07-22 08:56:34.988')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079393781261668352', N'2060785386585722880', N'1', N'0', N'1903486709602062336', N'2026-07-22 08:56:34.988', N'1903486709602062336', N'2026-07-22 08:56:34.988')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079393781261668352', N'2060785728597659648', N'1', N'1', N'1903486709602062336', N'2026-07-22 08:56:34.988', N'1903486709602062336', N'2026-07-22 08:56:34.988')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079393781261668352', N'2060785864845430784', N'1', N'1', N'1903486709602062336', N'2026-07-22 08:56:34.988', N'1903486709602062336', N'2026-07-22 08:56:34.988')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079393781261668352', N'2060786020718350336', N'1', N'1', N'1903486709602062336', N'2026-07-22 08:56:34.988', N'1903486709602062336', N'2026-07-22 08:56:34.988')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079393781261668352', N'2060786240139169792', N'0', N'1', N'1903486709602062336', N'2026-07-22 08:56:34.988', N'1903486709602062336', N'2026-07-22 08:56:34.988')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079393781261668352', N'2060786334641033216', N'1', N'0', N'1903486709602062336', N'2026-07-22 08:56:34.988', N'1903486709602062336', N'2026-07-22 08:56:34.988')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079393781261668352', N'2060786455856418816', N'1', N'0', N'1903486709602062336', N'2026-07-22 08:56:34.988', N'1903486709602062336', N'2026-07-22 08:56:34.988')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394103849783296', N'2059813328053735424', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:24.632', N'1903486709602062336', N'2026-07-23 16:30:24.632')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394103849783296', N'2066137776289615872', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:24.632', N'1903486709602062336', N'2026-07-23 16:30:24.632')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394103849783296', N'2059839522363019264', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:24.632', N'1903486709602062336', N'2026-07-23 16:30:24.632')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394103849783296', N'2079802896626618368', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:30:24.632', N'1903486709602062336', N'2026-07-23 16:30:24.632')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394103849783296', N'2059839903075799041', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:24.632', N'1903486709602062336', N'2026-07-23 16:30:24.632')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394103849783296', N'2066887051650928640', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:24.632', N'1903486709602062336', N'2026-07-23 16:30:24.632')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394103849783296', N'2060784098187808768', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:24.633', N'1903486709602062336', N'2026-07-23 16:30:24.633')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394103849783296', N'2060784667472302080', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:24.633', N'1903486709602062336', N'2026-07-23 16:30:24.633')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394103849783296', N'2060785386585722880', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:24.633', N'1903486709602062336', N'2026-07-23 16:30:24.633')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394103849783296', N'2060785728597659648', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:24.633', N'1903486709602062336', N'2026-07-23 16:30:24.633')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394103849783296', N'2060785864845430784', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:24.633', N'1903486709602062336', N'2026-07-23 16:30:24.633')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394103849783296', N'2060786020718350336', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:24.633', N'1903486709602062336', N'2026-07-23 16:30:24.633')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394103849783296', N'2080208377132224512', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:24.633', N'1903486709602062336', N'2026-07-23 16:30:24.633')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394103849783296', N'2060786240139169792', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:30:24.633', N'1903486709602062336', N'2026-07-23 16:30:24.633')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394103849783296', N'2060786334641033216', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:24.633', N'1903486709602062336', N'2026-07-23 16:30:24.633')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009890853346217984', N'2060786455856418816', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:29:05.353', N'1903486709602062336', N'2026-07-23 16:29:05.353')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009892923604340736', N'2060786455856418816', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:29:14.914', N'1903486709602062336', N'2026-07-23 16:29:14.914')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009897830268932096', N'2060786455856418816', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:29:35.345', N'1903486709602062336', N'2026-07-23 16:29:35.345')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009898117243211776', N'2060786455856418816', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:29:58.682', N'1903486709602062336', N'2026-07-23 16:29:58.682')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2029389483455156224', N'2060786455856418816', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:07.469', N'1903486709602062336', N'2026-07-23 16:30:07.469')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2036076248547069952', N'2060786455856418816', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:12.869', N'1903486709602062336', N'2026-07-23 16:30:12.869')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394103849783296', N'2060786455856418816', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:24.633', N'1903486709602062336', N'2026-07-23 16:30:24.633')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394196795559936', N'2059813328053735424', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:28.678', N'1903486709602062336', N'2026-07-23 16:30:28.678')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394196795559936', N'2066137776289615872', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:28.678', N'1903486709602062336', N'2026-07-23 16:30:28.678')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394196795559936', N'2059839522363019264', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:28.678', N'1903486709602062336', N'2026-07-23 16:30:28.678')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394196795559936', N'2079802896626618368', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:30:28.678', N'1903486709602062336', N'2026-07-23 16:30:28.678')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394196795559936', N'2059839903075799041', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:28.678', N'1903486709602062336', N'2026-07-23 16:30:28.678')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394196795559936', N'2066887051650928640', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:28.678', N'1903486709602062336', N'2026-07-23 16:30:28.678')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394196795559936', N'2060784098187808768', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:28.678', N'1903486709602062336', N'2026-07-23 16:30:28.678')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394196795559936', N'2060784667472302080', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:28.678', N'1903486709602062336', N'2026-07-23 16:30:28.678')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394196795559936', N'2060785386585722880', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:28.678', N'1903486709602062336', N'2026-07-23 16:30:28.678')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394196795559936', N'2060785728597659648', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:28.678', N'1903486709602062336', N'2026-07-23 16:30:28.678')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394196795559936', N'2060785864845430784', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:28.678', N'1903486709602062336', N'2026-07-23 16:30:28.678')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394196795559936', N'2060786020718350336', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:28.678', N'1903486709602062336', N'2026-07-23 16:30:28.678')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394196795559936', N'2080208377132224512', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:28.678', N'1903486709602062336', N'2026-07-23 16:30:28.678')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394196795559936', N'2060786240139169792', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:30:28.678', N'1903486709602062336', N'2026-07-23 16:30:28.678')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394196795559936', N'2060786334641033216', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:28.678', N'1903486709602062336', N'2026-07-23 16:30:28.678')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394196795559936', N'2060786455856418816', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:28.678', N'1903486709602062336', N'2026-07-23 16:30:28.678')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394286754992128', N'2059813328053735424', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:32.278', N'1903486709602062336', N'2026-07-23 16:30:32.278')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394286754992128', N'2066137776289615872', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:32.278', N'1903486709602062336', N'2026-07-23 16:30:32.278')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394286754992128', N'2059839522363019264', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:32.278', N'1903486709602062336', N'2026-07-23 16:30:32.278')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394286754992128', N'2079802896626618368', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:30:32.278', N'1903486709602062336', N'2026-07-23 16:30:32.278')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394286754992128', N'2059839903075799041', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:32.278', N'1903486709602062336', N'2026-07-23 16:30:32.278')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394286754992128', N'2066887051650928640', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:32.278', N'1903486709602062336', N'2026-07-23 16:30:32.278')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394286754992128', N'2060784098187808768', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:32.278', N'1903486709602062336', N'2026-07-23 16:30:32.278')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394286754992128', N'2060784667472302080', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:32.278', N'1903486709602062336', N'2026-07-23 16:30:32.278')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394286754992128', N'2060785386585722880', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:32.278', N'1903486709602062336', N'2026-07-23 16:30:32.278')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394286754992128', N'2060785728597659648', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:32.278', N'1903486709602062336', N'2026-07-23 16:30:32.278')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394286754992128', N'2060785864845430784', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:32.278', N'1903486709602062336', N'2026-07-23 16:30:32.278')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394286754992128', N'2060786020718350336', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:30:32.278', N'1903486709602062336', N'2026-07-23 16:30:32.278')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394286754992128', N'2080208377132224512', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:32.278', N'1903486709602062336', N'2026-07-23 16:30:32.278')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394286754992128', N'2060786240139169792', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:30:32.278', N'1903486709602062336', N'2026-07-23 16:30:32.278')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394286754992128', N'2060786334641033216', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:32.278', N'1903486709602062336', N'2026-07-23 16:30:32.278')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394286754992128', N'2060786455856418816', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:32.278', N'1903486709602062336', N'2026-07-23 16:30:32.278')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010240', N'2060786455856418816', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:35.087', N'1903486709602062336', N'2026-07-23 16:30:35.087')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010286', N'2060786455856418816', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:30:41.197', N'1903486709602062336', N'2026-07-23 16:30:41.197')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807667064410112', N'2079802606770851840', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:32:02.267', N'1903486709602062336', N'2026-07-23 16:32:02.267')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807667064410112', N'2079802708130402304', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:32:02.267', N'1903486709602062336', N'2026-07-23 16:32:02.267')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807667064410112', N'2079802831254196224', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:32:02.267', N'1903486709602062336', N'2026-07-23 16:32:02.267')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807667064410112', N'2079802976205148160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:32:02.267', N'1903486709602062336', N'2026-07-23 16:32:02.267')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807667064410112', N'2079803153322217472', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:32:02.267', N'1903486709602062336', N'2026-07-23 16:32:02.267')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807667064410112', N'2079803460731146240', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:32:02.267', N'1903486709602062336', N'2026-07-23 16:32:02.267')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807667064410112', N'2079803673143283712', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:32:02.267', N'1903486709602062336', N'2026-07-23 16:32:02.267')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807667064410112', N'2079804002605862912', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:32:02.267', N'1903486709602062336', N'2026-07-23 16:32:02.267')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807667064410112', N'2079807502907740160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:32:02.267', N'1903486709602062336', N'2026-07-23 16:32:02.267')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807667064410112', N'2080207951758495744', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:32:02.267', N'1903486709602062336', N'2026-07-23 16:32:02.267')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807667064410112', N'2080208102950572032', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:32:02.267', N'1903486709602062336', N'2026-07-23 16:32:02.267')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807667064410112', N'2080208155173851136', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:32:02.267', N'1903486709602062336', N'2026-07-23 16:32:02.267')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807667064410112', N'2080208205656494080', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:32:02.267', N'1903486709602062336', N'2026-07-23 16:32:02.267')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807827882414080', N'2079802606770851840', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:08.642', N'1903486709602062336', N'2026-07-23 16:38:08.642')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807827882414080', N'2079802708130402304', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:08.642', N'1903486709602062336', N'2026-07-23 16:38:08.642')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807827882414080', N'2079802831254196224', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:08.642', N'1903486709602062336', N'2026-07-23 16:38:08.642')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807827882414080', N'2079802976205148160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:08.642', N'1903486709602062336', N'2026-07-23 16:38:08.642')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807827882414080', N'2079803153322217472', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:08.642', N'1903486709602062336', N'2026-07-23 16:38:08.642')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807827882414080', N'2079803460731146240', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:38:08.642', N'1903486709602062336', N'2026-07-23 16:38:08.642')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807827882414080', N'2079803673143283712', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:08.642', N'1903486709602062336', N'2026-07-23 16:38:08.642')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807827882414080', N'2079804002605862912', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:08.642', N'1903486709602062336', N'2026-07-23 16:38:08.642')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807827882414080', N'2079807502907740160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:08.642', N'1903486709602062336', N'2026-07-23 16:38:08.642')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807827882414080', N'2080207951758495744', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:08.642', N'1903486709602062336', N'2026-07-23 16:38:08.642')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807827882414080', N'2080208102950572032', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:38:08.642', N'1903486709602062336', N'2026-07-23 16:38:08.642')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807827882414080', N'2080208155173851136', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:08.642', N'1903486709602062336', N'2026-07-23 16:38:08.642')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807827882414080', N'2080208205656494080', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:08.642', N'1903486709602062336', N'2026-07-23 16:38:08.642')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807978587951104', N'2079802606770851840', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:05.007', N'1903486709602062336', N'2026-07-23 16:38:05.007')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807978587951104', N'2079802708130402304', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:05.007', N'1903486709602062336', N'2026-07-23 16:38:05.007')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807978587951104', N'2079802831254196224', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:05.007', N'1903486709602062336', N'2026-07-23 16:38:05.007')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807978587951104', N'2079802976205148160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:05.007', N'1903486709602062336', N'2026-07-23 16:38:05.007')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807978587951104', N'2079803153322217472', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:05.007', N'1903486709602062336', N'2026-07-23 16:38:05.007')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807978587951104', N'2079803460731146240', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:38:05.007', N'1903486709602062336', N'2026-07-23 16:38:05.007')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807978587951104', N'2079803673143283712', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:05.007', N'1903486709602062336', N'2026-07-23 16:38:05.007')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807978587951104', N'2079804002605862912', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:05.007', N'1903486709602062336', N'2026-07-23 16:38:05.007')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807978587951104', N'2079807502907740160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:05.007', N'1903486709602062336', N'2026-07-23 16:38:05.007')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807978587951104', N'2080207951758495744', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:05.007', N'1903486709602062336', N'2026-07-23 16:38:05.007')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807978587951104', N'2080208102950572032', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:38:05.007', N'1903486709602062336', N'2026-07-23 16:38:05.007')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807978587951104', N'2080208155173851136', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:05.007', N'1903486709602062336', N'2026-07-23 16:38:05.007')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807978587951104', N'2080208205656494080', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:05.007', N'1903486709602062336', N'2026-07-23 16:38:05.007')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808600624205824', N'2079802606770851840', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:14.039', N'1903486709602062336', N'2026-07-23 16:38:14.039')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808600624205824', N'2079802708130402304', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:14.039', N'1903486709602062336', N'2026-07-23 16:38:14.039')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808600624205824', N'2079802831254196224', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:14.039', N'1903486709602062336', N'2026-07-23 16:38:14.039')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808600624205824', N'2079802976205148160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:14.039', N'1903486709602062336', N'2026-07-23 16:38:14.039')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808600624205824', N'2079803153322217472', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:14.039', N'1903486709602062336', N'2026-07-23 16:38:14.039')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808600624205824', N'2079803460731146240', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:38:14.039', N'1903486709602062336', N'2026-07-23 16:38:14.039')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808600624205824', N'2079803673143283712', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:14.039', N'1903486709602062336', N'2026-07-23 16:38:14.039')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808600624205824', N'2079804002605862912', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:14.039', N'1903486709602062336', N'2026-07-23 16:38:14.039')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808600624205824', N'2079807502907740160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:14.039', N'1903486709602062336', N'2026-07-23 16:38:14.039')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808600624205824', N'2080207951758495744', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:14.039', N'1903486709602062336', N'2026-07-23 16:38:14.039')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808600624205824', N'2080208102950572032', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:38:14.039', N'1903486709602062336', N'2026-07-23 16:38:14.039')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808600624205824', N'2080208155173851136', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:14.039', N'1903486709602062336', N'2026-07-23 16:38:14.039')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808600624205824', N'2080208205656494080', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:14.039', N'1903486709602062336', N'2026-07-23 16:38:14.039')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808724595249152', N'2079802606770851840', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:21.456', N'1903486709602062336', N'2026-07-23 16:38:21.456')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808724595249152', N'2079802708130402304', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:21.456', N'1903486709602062336', N'2026-07-23 16:38:21.456')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808724595249152', N'2079802831254196224', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:21.456', N'1903486709602062336', N'2026-07-23 16:38:21.456')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808724595249152', N'2079802976205148160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:21.456', N'1903486709602062336', N'2026-07-23 16:38:21.456')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808724595249152', N'2079803153322217472', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:21.456', N'1903486709602062336', N'2026-07-23 16:38:21.456')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808724595249152', N'2079803460731146240', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:38:21.456', N'1903486709602062336', N'2026-07-23 16:38:21.456')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808724595249152', N'2079803673143283712', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:21.456', N'1903486709602062336', N'2026-07-23 16:38:21.456')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808724595249152', N'2079804002605862912', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:21.456', N'1903486709602062336', N'2026-07-23 16:38:21.456')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808724595249152', N'2079807502907740160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:21.456', N'1903486709602062336', N'2026-07-23 16:38:21.456')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808724595249152', N'2080207951758495744', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:21.456', N'1903486709602062336', N'2026-07-23 16:38:21.456')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808724595249152', N'2080208102950572032', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:38:21.456', N'1903486709602062336', N'2026-07-23 16:38:21.456')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808724595249152', N'2080208155173851136', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:21.456', N'1903486709602062336', N'2026-07-23 16:38:21.456')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808724595249152', N'2080208205656494080', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:21.456', N'1903486709602062336', N'2026-07-23 16:38:21.456')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808873866334208', N'2079802606770851840', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:24.495', N'1903486709602062336', N'2026-07-23 16:38:24.495')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808873866334208', N'2079802708130402304', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:24.495', N'1903486709602062336', N'2026-07-23 16:38:24.495')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808873866334208', N'2079802831254196224', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:24.495', N'1903486709602062336', N'2026-07-23 16:38:24.495')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808873866334208', N'2079802976205148160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:24.495', N'1903486709602062336', N'2026-07-23 16:38:24.495')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808873866334208', N'2079803153322217472', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:24.495', N'1903486709602062336', N'2026-07-23 16:38:24.495')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808873866334208', N'2079803460731146240', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:38:24.495', N'1903486709602062336', N'2026-07-23 16:38:24.495')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808873866334208', N'2079803673143283712', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:24.495', N'1903486709602062336', N'2026-07-23 16:38:24.495')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808873866334208', N'2079804002605862912', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:24.495', N'1903486709602062336', N'2026-07-23 16:38:24.495')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808873866334208', N'2079807502907740160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:24.495', N'1903486709602062336', N'2026-07-23 16:38:24.495')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808873866334208', N'2080207951758495744', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:24.495', N'1903486709602062336', N'2026-07-23 16:38:24.495')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808873866334208', N'2080208102950572032', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:38:24.495', N'1903486709602062336', N'2026-07-23 16:38:24.495')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808873866334208', N'2080208155173851136', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:24.495', N'1903486709602062336', N'2026-07-23 16:38:24.495')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808873866334208', N'2080208205656494080', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:24.495', N'1903486709602062336', N'2026-07-23 16:38:24.495')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809045400784896', N'2079802606770851840', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:58.219', N'1903486709602062336', N'2026-07-23 16:38:58.219')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809045400784896', N'2079802708130402304', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:58.219', N'1903486709602062336', N'2026-07-23 16:38:58.219')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809045400784896', N'2079802831254196224', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:58.219', N'1903486709602062336', N'2026-07-23 16:38:58.219')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809045400784896', N'2079802976205148160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:58.219', N'1903486709602062336', N'2026-07-23 16:38:58.219')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809045400784896', N'2079803153322217472', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:58.219', N'1903486709602062336', N'2026-07-23 16:38:58.219')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809045400784896', N'2079803460731146240', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:38:58.219', N'1903486709602062336', N'2026-07-23 16:38:58.219')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809045400784896', N'2079803673143283712', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:58.219', N'1903486709602062336', N'2026-07-23 16:38:58.219')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809045400784896', N'2079804002605862912', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:58.219', N'1903486709602062336', N'2026-07-23 16:38:58.219')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809045400784896', N'2079807502907740160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:38:58.219', N'1903486709602062336', N'2026-07-23 16:38:58.219')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809045400784896', N'2080207951758495744', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:58.219', N'1903486709602062336', N'2026-07-23 16:38:58.219')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809045400784896', N'2080208102950572032', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:38:58.219', N'1903486709602062336', N'2026-07-23 16:38:58.219')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809045400784896', N'2080208155173851136', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:58.219', N'1903486709602062336', N'2026-07-23 16:38:58.219')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809045400784896', N'2080208205656494080', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:38:58.219', N'1903486709602062336', N'2026-07-23 16:38:58.219')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809245062238208', N'2079802606770851840', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:39:48.128', N'1903486709602062336', N'2026-07-23 16:39:48.128')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809245062238208', N'2079802708130402304', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:39:48.128', N'1903486709602062336', N'2026-07-23 16:39:48.128')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809245062238208', N'2079802831254196224', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:39:48.128', N'1903486709602062336', N'2026-07-23 16:39:48.128')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809245062238208', N'2079802976205148160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:39:48.128', N'1903486709602062336', N'2026-07-23 16:39:48.128')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809245062238208', N'2079803153322217472', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:39:48.128', N'1903486709602062336', N'2026-07-23 16:39:48.128')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809245062238208', N'2079803460731146240', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:39:48.128', N'1903486709602062336', N'2026-07-23 16:39:48.128')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809245062238208', N'2079803673143283712', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:39:48.128', N'1903486709602062336', N'2026-07-23 16:39:48.128')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809245062238208', N'2079804002605862912', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:39:48.128', N'1903486709602062336', N'2026-07-23 16:39:48.128')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809245062238208', N'2079807502907740160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:39:48.128', N'1903486709602062336', N'2026-07-23 16:39:48.128')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809245062238208', N'2080207951758495744', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:39:48.128', N'1903486709602062336', N'2026-07-23 16:39:48.128')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809245062238208', N'2080208102950572032', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:39:48.128', N'1903486709602062336', N'2026-07-23 16:39:48.128')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809245062238208', N'2080208155173851136', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:39:48.128', N'1903486709602062336', N'2026-07-23 16:39:48.128')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809245062238208', N'2080208205656494080', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:39:48.128', N'1903486709602062336', N'2026-07-23 16:39:48.128')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809399479734272', N'2079802606770851840', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:41:25.864', N'1903486709602062336', N'2026-07-23 16:41:25.864')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809399479734272', N'2079802708130402304', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:41:25.864', N'1903486709602062336', N'2026-07-23 16:41:25.864')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809399479734272', N'2079802831254196224', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:41:25.864', N'1903486709602062336', N'2026-07-23 16:41:25.864')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809399479734272', N'2079802976205148160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:41:25.864', N'1903486709602062336', N'2026-07-23 16:41:25.864')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809399479734272', N'2079803153322217472', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:41:25.864', N'1903486709602062336', N'2026-07-23 16:41:25.864')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809399479734272', N'2079803460731146240', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:41:25.864', N'1903486709602062336', N'2026-07-23 16:41:25.864')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809399479734272', N'2079803673143283712', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:41:25.864', N'1903486709602062336', N'2026-07-23 16:41:25.864')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809399479734272', N'2079804002605862912', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:41:25.864', N'1903486709602062336', N'2026-07-23 16:41:25.864')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809399479734272', N'2079807502907740160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:41:25.864', N'1903486709602062336', N'2026-07-23 16:41:25.864')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809399479734272', N'2080207951758495744', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:41:25.864', N'1903486709602062336', N'2026-07-23 16:41:25.864')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809399479734272', N'2080208102950572032', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:41:25.864', N'1903486709602062336', N'2026-07-23 16:41:25.864')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809399479734272', N'2080208155173851136', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:41:25.864', N'1903486709602062336', N'2026-07-23 16:41:25.864')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809399479734272', N'2080208205656494080', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:41:25.864', N'1903486709602062336', N'2026-07-23 16:41:25.864')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809544439074816', N'2079802606770851840', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:42:46.885', N'1903486709602062336', N'2026-07-23 16:42:46.885')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809544439074816', N'2079802708130402304', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:42:46.885', N'1903486709602062336', N'2026-07-23 16:42:46.885')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809544439074816', N'2079802831254196224', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:42:46.885', N'1903486709602062336', N'2026-07-23 16:42:46.885')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809544439074816', N'2079802976205148160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:42:46.885', N'1903486709602062336', N'2026-07-23 16:42:46.885')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809544439074816', N'2079803153322217472', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:42:46.885', N'1903486709602062336', N'2026-07-23 16:42:46.885')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809544439074816', N'2079803460731146240', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:42:46.885', N'1903486709602062336', N'2026-07-23 16:42:46.885')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809544439074816', N'2079803673143283712', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:42:46.885', N'1903486709602062336', N'2026-07-23 16:42:46.885')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809544439074816', N'2079804002605862912', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:42:46.885', N'1903486709602062336', N'2026-07-23 16:42:46.885')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809544439074816', N'2079807502907740160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:42:46.885', N'1903486709602062336', N'2026-07-23 16:42:46.885')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809544439074816', N'2080207951758495744', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:42:46.885', N'1903486709602062336', N'2026-07-23 16:42:46.885')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809544439074816', N'2080208102950572032', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:42:46.885', N'1903486709602062336', N'2026-07-23 16:42:46.885')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809544439074816', N'2080208155173851136', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:42:46.885', N'1903486709602062336', N'2026-07-23 16:42:46.885')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809544439074816', N'2080208205656494080', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:42:46.885', N'1903486709602062336', N'2026-07-23 16:42:46.885')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809778514792448', N'2079802606770851840', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:43:02.206', N'1903486709602062336', N'2026-07-23 16:43:02.206')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809778514792448', N'2079802708130402304', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:43:02.206', N'1903486709602062336', N'2026-07-23 16:43:02.206')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809778514792448', N'2079802831254196224', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:43:02.206', N'1903486709602062336', N'2026-07-23 16:43:02.206')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809778514792448', N'2079802976205148160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:43:02.206', N'1903486709602062336', N'2026-07-23 16:43:02.206')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809778514792448', N'2079803153322217472', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:43:02.206', N'1903486709602062336', N'2026-07-23 16:43:02.206')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809778514792448', N'2079803460731146240', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:43:02.206', N'1903486709602062336', N'2026-07-23 16:43:02.206')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809778514792448', N'2079803673143283712', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:43:02.206', N'1903486709602062336', N'2026-07-23 16:43:02.206')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809778514792448', N'2079804002605862912', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:43:02.206', N'1903486709602062336', N'2026-07-23 16:43:02.206')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809778514792448', N'2079807502907740160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:43:02.206', N'1903486709602062336', N'2026-07-23 16:43:02.206')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809778514792448', N'2080207951758495744', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:43:02.206', N'1903486709602062336', N'2026-07-23 16:43:02.206')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809778514792448', N'2080208102950572032', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:43:02.206', N'1903486709602062336', N'2026-07-23 16:43:02.206')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809778514792448', N'2080208155173851136', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:43:02.206', N'1903486709602062336', N'2026-07-23 16:43:02.206')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809778514792448', N'2080208205656494080', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:43:02.206', N'1903486709602062336', N'2026-07-23 16:43:02.206')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809906873077760', N'2079802606770851840', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:43:19.138', N'1903486709602062336', N'2026-07-23 16:43:19.138')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809906873077760', N'2079802708130402304', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:43:19.138', N'1903486709602062336', N'2026-07-23 16:43:19.138')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809906873077760', N'2079802831254196224', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:43:19.138', N'1903486709602062336', N'2026-07-23 16:43:19.138')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809906873077760', N'2079802976205148160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:43:19.138', N'1903486709602062336', N'2026-07-23 16:43:19.138')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809906873077760', N'2079803153322217472', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:43:19.138', N'1903486709602062336', N'2026-07-23 16:43:19.138')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809906873077760', N'2079803460731146240', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:43:19.138', N'1903486709602062336', N'2026-07-23 16:43:19.138')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809906873077760', N'2079803673143283712', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:43:19.138', N'1903486709602062336', N'2026-07-23 16:43:19.138')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809906873077760', N'2079804002605862912', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:43:19.138', N'1903486709602062336', N'2026-07-23 16:43:19.138')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809906873077760', N'2079807502907740160', N'1', N'1', N'1903486709602062336', N'2026-07-23 16:43:19.138', N'1903486709602062336', N'2026-07-23 16:43:19.138')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809906873077760', N'2080207951758495744', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:43:19.138', N'1903486709602062336', N'2026-07-23 16:43:19.138')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809906873077760', N'2080208102950572032', N'0', N'1', N'1903486709602062336', N'2026-07-23 16:43:19.138', N'1903486709602062336', N'2026-07-23 16:43:19.138')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809906873077760', N'2080208155173851136', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:43:19.138', N'1903486709602062336', N'2026-07-23 16:43:19.138')
GO

INSERT INTO [Form].[StepFieldPermission] ([StepId], [FieldId], [IsVisible], [IsDisabled], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809906873077760', N'2080208205656494080', N'1', N'0', N'1903486709602062336', N'2026-07-23 16:43:19.138', N'1903486709602062336', N'2026-07-23 16:43:19.138')
GO


-- ----------------------------
-- Table structure for WorkflowRule
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Form].[WorkflowRule]') AND type IN ('U'))
	DROP TABLE [Form].[WorkflowRule]
GO

CREATE TABLE [Form].[WorkflowRule] (
  [RuleId] bigint  NOT NULL,
  [RuleNameCn] nvarchar(20) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [RuleNameEn] nvarchar(50) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [FormTypeId] bigint  NOT NULL,
  [PositionId] bigint  NULL,
  [Guidance] nvarchar(20) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NULL,
  [Version] varchar(20) COLLATE Chinese_PRC_CI_AS  NOT NULL,
  [EffectiveStartDate] date  NOT NULL,
  [EffectiveEndDate] date  NULL,
  [SortOrder] int  NOT NULL,
  [CreatedBy] bigint  NOT NULL,
  [CreatedDate] datetime2(3)  NOT NULL,
  [ModifiedBy] bigint  NULL,
  [ModifiedDate] datetime2(3)  NULL
)
GO

ALTER TABLE [Form].[WorkflowRule] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'规则Id',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRule',
'COLUMN', N'RuleId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'规则名称（中文）',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRule',
'COLUMN', N'RuleNameCn'
GO

EXEC sp_addextendedproperty
'MS_Description', N'规则名称（英文）',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRule',
'COLUMN', N'RuleNameEn'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单类别Id',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRule',
'COLUMN', N'FormTypeId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'职级Id',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRule',
'COLUMN', N'PositionId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'导向',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRule',
'COLUMN', N'Guidance'
GO

EXEC sp_addextendedproperty
'MS_Description', N'版本',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRule',
'COLUMN', N'Version'
GO

EXEC sp_addextendedproperty
'MS_Description', N'生效日期',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRule',
'COLUMN', N'EffectiveStartDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'失效日期',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRule',
'COLUMN', N'EffectiveEndDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'排序',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRule',
'COLUMN', N'SortOrder'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRule',
'COLUMN', N'CreatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRule',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改人',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRule',
'COLUMN', N'ModifiedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改时间',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRule',
'COLUMN', N'ModifiedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'流程规则表',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRule'
GO


-- ----------------------------
-- Records of WorkflowRule
-- ----------------------------
INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2046850267772751872', N'组长 - 小于3天', N'Team Leader - Less than 3 days', N'1987217256446300160', N'1351602631784529920', N'LessOver3', N'V1.1', N'2026-01-01', NULL, N'1', N'1903486709602062336', N'2026-04-22 15:15:07.133', N'1903486709602062336', N'2026-07-16 18:31:54.670')
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2046860073602519040', N'其他', N'Other', N'1987217256446300160', NULL, NULL, N'V1.1', N'2026-01-01', NULL, N'50', N'1903486709602062336', N'2026-04-22 15:54:05.027', N'1903486709602062336', N'2026-07-16 18:45:36.410')
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051185303204532224', N'组长 - 大于3天', N'Team Leader - More than 3 days', N'1987217256446300160', N'1351602631784529920', N'MoreOver3', N'V1.1', N'2026-01-01', NULL, N'2', N'1903486709602062336', N'2026-05-04 14:21:00.120', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051186271467999232', N'科长 - 小于3天', N'Section Chief - Less than 3 days', N'1987217256446300160', N'1351600746193223680', N'LessOver3', N'V1.1', N'2026-01-01', NULL, N'3', N'1903486709602062336', N'2026-05-04 14:24:50.973', N'1903486709602062336', N'2026-05-04 14:32:53.470')
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051186400535121920', N'科长 - 大于3天', N'Section Chief - More than 3 days', N'1987217256446300160', N'1351600746193223680', N'MoreOver3', N'V1.1', N'2026-01-01', NULL, N'4', N'1903486709602062336', N'2026-05-04 14:25:21.747', N'1903486709602062336', N'2026-07-16 18:44:24.993')
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051259795381555200', N'副理 - 小于3天', N'Assistant Manager - Less than 3 days', N'1987217256446300160', N'1351592278136717312', N'LessOver3', N'V1.1', N'2026-01-01', NULL, N'5', N'1903486709602062336', N'2026-05-04 19:17:00.440', N'1903486709602062336', N'2026-07-21 18:41:02.287')
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051260041411039232', N'副理 - 大于3天', N'Assistant Manager - More than 3 days', N'1987217256446300160', N'1351592278136717312', N'MoreOver3', N'V1.1', N'2026-01-01', NULL, N'6', N'1903486709602062336', N'2026-05-04 19:17:59.100', N'1903486709602062336', N'2026-07-16 18:42:45.703')
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079485052013645824', N'经理 - 大于3天', N'Manager - More than 3 days', N'1987217256446300160', N'1351585319710883840', N'MoreOver3', N'V1.1', N'2026-01-01', NULL, N'1', N'1903486709602062336', N'2026-07-21 16:34:06.023', N'1903486709602062336', N'2026-07-21 16:34:37.563')
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079485332910379008', N'经理 - 小于3天', N'Manager - Less than 3 days', N'1987217256446300160', N'1351585319710883840', N'LessOver3', N'V1.1', N'2026-01-01', NULL, N'1', N'1903486709602062336', N'2026-07-21 16:35:12.993', N'1903486709602062336', N'2026-07-21 16:35:30.033')
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079733379242266624', N'资深经理 - 小于3天', N'Senior Manager - Less than 3 days', N'1987217256446300160', N'1351584156689104896', N'LessOver3', N'V1.1', N'2026-01-01', NULL, N'1', N'1903486709602062336', N'2026-07-22 09:00:51.847', N'1903486709602062336', N'2026-07-22 11:13:05.140')
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079733578282962944', N'资深经理 - 大于3天', N'Senior Manager - More than 3 days', N'1987217256446300160', N'1351584156689104896', N'MoreOver3', N'V1.1', N'2026-01-01', NULL, N'1', N'1903486709602062336', N'2026-07-22 09:01:39.300', N'1903486709602062336', N'2026-07-22 11:13:00.843')
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079766357347536896', N'厂长', N'Factory Manager', N'1987217256446300160', N'1351584014896463872', NULL, N'V1.1', N'2026-01-01', NULL, N'1', N'1903486709602062336', N'2026-07-22 11:11:54.440', N'1903486709602062336', N'2026-07-22 11:16:21.117')
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079767995848200192', N'协理', N'Associate', N'1987217256446300160', N'1351583636813512704', NULL, N'V1.1', N'2026-01-01', NULL, N'1', N'1903486709602062336', N'2026-07-22 11:18:25.090', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079768821333364736', N'副总', N'Vice President', N'1987217256446300160', N'1351583500196642816', NULL, N'V1.1', N'2026-01-01', NULL, N'1', N'1903486709602062336', N'2026-07-22 11:21:41.900', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079811193538744320', N'组长', N'Team leader', N'2074764225741459456', N'1351602631784529920', NULL, N'V1.1', N'2026-01-01', NULL, N'1', N'1903486709602062336', N'2026-07-22 14:10:04.220', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079823631856308224', N'副理', N'Deputy Manager', N'2074764225741459456', N'1351592278136717312', NULL, N'V1.1', N'2026-01-01', NULL, N'1', N'1903486709602062336', N'2026-07-22 14:59:29.750', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079823989164871680', N'师一', N'Engineer I', N'1987217256446300160', N'1351602976027836416', NULL, N'V1.1', N'2026-01-01', NULL, N'1', N'1903486709602062336', N'2026-07-22 15:00:54.937', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079824160229560320', N'师二', N'Engineer II', N'1987217256446300160', N'1351602771312246784', NULL, N'V1.1', N'2026-01-01', NULL, N'1', N'1903486709602062336', N'2026-07-22 15:01:35.720', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079825038730727424', N'师二', N'Engineer II', N'2074764225741459456', N'1351602771312246784', NULL, N'V1.1', N'2026-01-01', NULL, N'1', N'1903486709602062336', N'2026-07-22 15:05:05.173', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079825111464153088', N'师一', N'Engineer I', N'2074764225741459456', N'1351602976027836416', NULL, N'V1.1', N'2026-01-01', NULL, N'1', N'1903486709602062336', N'2026-07-22 15:05:22.513', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079825705708949504', N'科长', N'Section Chief', N'2074764225741459456', N'1351600746193223680', NULL, N'V1.1', N'2026-01-01', NULL, N'1', N'1903486709602062336', N'2026-07-22 15:07:44.190', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079826769581576192', N'经理', N'Manager', N'2074764225741459456', N'1351585319710883840', NULL, N'V1.1', N'2026-01-01', NULL, N'1', N'1903486709602062336', N'2026-07-22 15:11:57.840', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079827087866335232', N'资深经理', N'Senior Manager', N'2074764225741459456', N'1351584156689104896', NULL, N'V1.1', N'2026-01-01', NULL, N'1', N'1903486709602062336', N'2026-07-22 15:13:13.723', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079827173505634304', N'厂长', N'Factory Director', N'2074764225741459456', N'1351584014896463872', NULL, N'V1.1', N'2026-01-01', NULL, N'1', N'1903486709602062336', N'2026-07-22 15:13:34.143', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079830880456675328', N'协理', N'Associate', N'2074764225741459456', N'1351583636813512704', NULL, N'V1.1', N'2026-01-01', NULL, N'1', N'1903486709602062336', N'2026-07-22 15:28:17.950', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRule] ([RuleId], [RuleNameCn], [RuleNameEn], [FormTypeId], [PositionId], [Guidance], [Version], [EffectiveStartDate], [EffectiveEndDate], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079831015152553984', N'副总', N'Vice President', N'2074764225741459456', N'1351583500196642816', NULL, N'V1.1', N'2026-01-01', NULL, N'1', N'1903486709602062336', N'2026-07-22 15:28:50.063', NULL, NULL)
GO


-- ----------------------------
-- Table structure for WorkflowRuleStep
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Form].[WorkflowRuleStep]') AND type IN ('U'))
	DROP TABLE [Form].[WorkflowRuleStep]
GO

CREATE TABLE [Form].[WorkflowRuleStep] (
  [RuleId] bigint  NOT NULL,
  [CurrentStepId] bigint  NOT NULL,
  [NextStepId] bigint  NULL,
  [Guidance] nvarchar(20) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NULL,
  [SortOrder] int  NULL,
  [CreatedBy] bigint  NULL,
  [CreatedDate] datetime2(3)  NULL,
  [ModifiedBy] bigint  NULL,
  [ModifiedDate] datetime2(3)  NULL
)
GO

ALTER TABLE [Form].[WorkflowRuleStep] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'规则Id',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRuleStep',
'COLUMN', N'RuleId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'当前步骤Id',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRuleStep',
'COLUMN', N'CurrentStepId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'下一步骤Id',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRuleStep',
'COLUMN', N'NextStepId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'导向',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRuleStep',
'COLUMN', N'Guidance'
GO

EXEC sp_addextendedproperty
'MS_Description', N'排序',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRuleStep',
'COLUMN', N'SortOrder'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRuleStep',
'COLUMN', N'CreatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRuleStep',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改人',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRuleStep',
'COLUMN', N'ModifiedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改时间',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRuleStep',
'COLUMN', N'ModifiedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'规则步骤表',
'SCHEMA', N'Form',
'TABLE', N'WorkflowRuleStep'
GO


-- ----------------------------
-- Records of WorkflowRuleStep
-- ----------------------------
INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009897830268932096', N'2009890853346217984', N'2009892923604340736', NULL, N'1', N'1903486709602062336', N'2026-04-23 11:39:44.170', N'1903486709602062336', N'2026-05-02 23:20:36.180')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2046850267772751872', N'2009897830268932096', N'2032353104544010240', NULL, N'2', N'1903486709602062336', N'2026-04-23 11:57:27.710', N'1903486709602062336', N'2026-06-21 20:12:05.900')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2046850267772751872', N'2032353104544010240', NULL, N'ProcessLeaveRequest', N'3', N'1903486709602062336', N'2026-04-23 11:58:54.023', N'1903486709602062336', N'2026-07-21 11:24:10.383')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2046860073602519040', N'2009890853346217984', N'2009892923604340736', NULL, N'1', N'1903486709602062336', N'2026-05-03 19:55:38.247', N'1903486709602062336', N'2026-05-03 19:55:44.140')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2046860073602519040', N'2009892923604340736', N'2009897830268932096', NULL, N'2', N'1903486709602062336', N'2026-05-03 19:55:53.800', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2046860073602519040', N'2009897830268932096', N'2009898117243211776', NULL, N'3', N'1903486709602062336', N'2026-05-03 19:59:09.177', N'1903486709602062336', N'2026-05-03 19:59:13.263')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2046860073602519040', N'2009898117243211776', N'2029389483455156224', NULL, N'4', N'1903486709602062336', N'2026-05-03 19:59:22.433', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051185303204532224', N'2009890853346217984', N'2009897830268932096', NULL, N'1', N'1903486709602062336', N'2026-05-04 14:25:44.423', N'1903486709602062336', N'2026-06-22 20:02:00.200')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051185303204532224', N'2009897830268932096', N'2009898117243211776', NULL, N'2', N'1903486709602062336', N'2026-05-04 14:26:07.183', N'1903486709602062336', N'2026-06-22 20:02:09.230')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051185303204532224', N'2009898117243211776', N'2032353104544010240', NULL, N'3', N'1903486709602062336', N'2026-05-04 14:26:13.737', N'1903486709602062336', N'2026-06-22 20:02:11.670')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051186271467999232', N'2009890853346217984', N'2009898117243211776', NULL, N'1', N'1903486709602062336', N'2026-05-04 14:35:44.930', N'1903486709602062336', N'2026-07-21 10:42:07.277')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051259795381555200', N'2009890853346217984', N'2009898117243211776', NULL, N'1', N'1903486709602062336', N'2026-07-21 10:45:34.123', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051259795381555200', N'2009898117243211776', N'2029389483455156224', NULL, N'2', N'1903486709602062336', N'2026-07-21 10:45:53.010', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051186271467999232', N'2032353104544010240', NULL, N'ProcessLeaveRequest', N'3', N'1903486709602062336', N'2026-05-04 14:36:34.560', N'1903486709602062336', N'2026-07-21 11:24:15.920')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2046850267772751872', N'2009890853346217984', N'2009897830268932096', NULL, N'1', N'1903486709602062336', N'2026-05-05 12:34:36.897', N'1903486709602062336', N'2026-06-21 20:12:12.133')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051186400535121920', N'2009890853346217984', N'2009898117243211776', NULL, N'1', N'1903486709602062336', N'2026-05-05 15:34:08.673', N'1903486709602062336', N'2026-07-21 10:44:32.267')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051259795381555200', N'2029389483455156224', N'2032353104544010286', NULL, N'3', N'1903486709602062336', N'2026-07-21 10:46:01.353', N'1903486709602062336', N'2026-07-21 10:47:32.687')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051186400535121920', N'2032353104544010240', NULL, N'ProcessLeaveRequest', N'4', N'1903486709602062336', N'2026-05-05 15:36:52.850', N'1903486709602062336', N'2026-07-21 11:24:26.753')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2046860073602519040', N'2029389483455156224', N'2032353104544010240', NULL, N'5', N'1903486709602062336', N'2026-05-03 19:59:28.767', N'1903486709602062336', N'2026-05-03 19:59:33.143')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2046860073602519040', N'2032353104544010240', N'2032353104544010286', NULL, N'6', N'1903486709602062336', N'2026-05-03 19:59:42.087', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2046860073602519040', N'2032353104544010286', NULL, N'ProcessLeaveForm', N'7', N'1903486709602062336', N'2026-05-03 19:59:47.373', N'1903486709602062336', N'2026-05-03 19:59:52.553')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051185303204532224', N'2032353104544010240', NULL, N'ProcessLeaveRequest', N'4', N'1903486709602062336', N'2026-05-04 14:26:28.063', N'1903486709602062336', N'2026-07-21 11:24:05.850')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051186271467999232', N'2009898117243211776', N'2032353104544010240', NULL, N'2', N'1903486709602062336', N'2026-05-04 14:36:24.513', N'1903486709602062336', N'2026-07-21 10:42:29.570')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051259795381555200', N'2032353104544010286', NULL, N'ProcessLeaveRequest', N'4', N'1903486709602062336', N'2026-07-21 10:47:57.153', N'1903486709602062336', N'2026-07-21 11:24:40.923')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051186400535121920', N'2009898117243211776', N'2029389483455156224', NULL, N'2', N'1903486709602062336', N'2026-05-05 15:34:32.307', N'1903486709602062336', N'2026-07-21 10:44:39.790')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051186400535121920', N'2029389483455156224', N'2032353104544010240', NULL, N'3', N'1903486709602062336', N'2026-05-05 15:34:40.330', N'1903486709602062336', N'2026-07-21 10:44:46.287')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051260041411039232', N'2009890853346217984', N'2009898117243211776', NULL, N'1', N'1903486709602062336', N'2026-07-21 10:48:26.740', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051260041411039232', N'2009898117243211776', N'2029389483455156224', NULL, N'2', N'1903486709602062336', N'2026-07-21 10:48:41.400', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051260041411039232', N'2036076248547069952', N'2032353104544010286', NULL, N'4', N'1903486709602062336', N'2026-07-21 10:49:05.400', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079485332910379008', N'2029389483455156224', N'2036076248547069952', NULL, N'2', N'1903486709602062336', N'2026-07-21 16:43:07.097', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051260041411039232', N'2029389483455156224', N'2036076248547069952', NULL, N'3', N'1903486709602062336', N'2026-07-21 10:48:51.703', N'1903486709602062336', N'2026-07-21 10:48:55.200')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2051260041411039232', N'2032353104544010286', NULL, N'ProcessLeaveRequest', N'5', N'1903486709602062336', N'2026-07-21 10:49:13.123', N'1903486709602062336', N'2026-07-21 11:33:54.637')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079485052013645824', N'2009890853346217984', N'2029389483455156224', NULL, N'1', N'1903486709602062336', N'2026-07-21 16:40:48.483', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079485052013645824', N'2029389483455156224', N'2036076248547069952', NULL, N'2', N'1903486709602062336', N'2026-07-21 16:41:09.670', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079485052013645824', N'2036076248547069952', N'2079393781261668352', NULL, N'3', N'1903486709602062336', N'2026-07-21 16:41:27.937', N'1903486709602062336', N'2026-07-21 16:41:42.210')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079485052013645824', N'2079393781261668352', N'2032353104544010286', NULL, N'4', N'1903486709602062336', N'2026-07-21 16:41:53.573', N'1903486709602062336', N'2026-07-21 16:41:58.877')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079485052013645824', N'2032353104544010286', NULL, N'ProcessLeaveRequest', N'5', N'1903486709602062336', N'2026-07-21 16:42:13.040', N'1903486709602062336', N'2026-07-21 16:42:17.630')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079485332910379008', N'2009890853346217984', N'2029389483455156224', NULL, N'1', N'1903486709602062336', N'2026-07-21 16:42:55.827', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079485332910379008', N'2036076248547069952', N'2032353104544010286', NULL, N'3', N'1903486709602062336', N'2026-07-21 16:43:20.927', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079485332910379008', N'2032353104544010286', NULL, N'ProcessLeaveRequest', N'4', N'1903486709602062336', N'2026-07-21 16:43:32.170', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079733379242266624', N'2009890853346217984', N'2036076248547069952', NULL, N'1', N'1903486709602062336', N'2026-07-22 09:02:18.587', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079733379242266624', N'2036076248547069952', N'2032353104544010286', NULL, N'2', N'1903486709602062336', N'2026-07-22 09:02:38.670', N'1903486709602062336', N'2026-07-22 09:02:46.957')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079733379242266624', N'2032353104544010286', NULL, N'ProcessLeaveRequest', N'3', N'1903486709602062336', N'2026-07-22 09:02:58.363', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079733578282962944', N'2079393781261668352', N'2032353104544010286', NULL, N'3', N'1903486709602062336', N'2026-07-22 09:04:02.883', N'1903486709602062336', N'2026-07-22 09:04:07.177')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079733578282962944', N'2032353104544010286', NULL, N'ProcessLeaveRequest', N'4', N'1903486709602062336', N'2026-07-22 09:04:12.723', N'1903486709602062336', N'2026-07-22 09:04:16.230')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079766357347536896', N'2009890853346217984', N'2079393781261668352', NULL, N'1', N'1903486709602062336', N'2026-07-22 11:15:16.900', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079766357347536896', N'2079393781261668352', N'2079394103849783296', NULL, N'2', N'1903486709602062336', N'2026-07-22 11:15:25.103', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079766357347536896', N'2079394103849783296', N'2079394196795559936', NULL, N'3', N'1903486709602062336', N'2026-07-22 11:15:52.440', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079766357347536896', N'2079394196795559936', NULL, N'ProcessLeaveRequest', N'4', N'1903486709602062336', N'2026-07-22 11:16:51.543', N'1903486709602062336', N'2026-07-22 11:16:58.503')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079767995848200192', N'2009890853346217984', N'2079394103849783296', NULL, N'1', N'1903486709602062336', N'2026-07-22 11:19:34.360', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079767995848200192', N'2079394196795559936', N'2079394286754992128', NULL, N'3', N'1903486709602062336', N'2026-07-22 11:20:00.733', N'1903486709602062336', N'2026-07-22 11:20:22.137')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079767995848200192', N'2079394103849783296', N'2079394196795559936', NULL, N'2', N'1903486709602062336', N'2026-07-22 11:20:18.220', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079767995848200192', N'2079394286754992128', NULL, N'ProcessLeaveRequest', N'4', N'1903486709602062336', N'2026-07-22 11:20:39.923', N'1903486709602062336', N'2026-07-22 11:20:44.420')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079768821333364736', N'2009890853346217984', N'2079394196795559936', NULL, N'1', N'1903486709602062336', N'2026-07-22 11:22:12.103', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079768821333364736', N'2079394196795559936', N'2079394286754992128', NULL, N'2', N'1903486709602062336', N'2026-07-22 11:23:36.240', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079768821333364736', N'2079394286754992128', NULL, N'ProcessLeaveRequest', N'3', N'1903486709602062336', N'2026-07-22 11:23:59.240', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079811193538744320', N'2079807667064410112', N'2079807978587951104', NULL, N'1', N'1903486709602062336', N'2026-07-22 14:10:29.290', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079811193538744320', N'2079809778514792448', N'2079809906873077760', NULL, N'3', N'1903486709602062336', N'2026-07-22 14:12:55.997', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079824160229560320', N'2009892923604340736', N'2032353104544010240', NULL, N'2', N'1903486709602062336', N'2026-07-22 15:03:25.777', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079824160229560320', N'2032353104544010240', NULL, N'ProcessLeaveRequest', N'3', N'1903486709602062336', N'2026-07-22 15:03:34.953', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079825111464153088', N'2079807667064410112', N'2079807827882414080', NULL, N'1', N'1903486709602062336', N'2026-07-22 15:05:37.430', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079825111464153088', N'2079809778514792448', NULL, N'ProcessLeaveCancell', N'3', N'1903486709602062336', N'2026-07-22 15:06:04.430', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079825038730727424', N'2079807667064410112', N'2079807827882414080', NULL, N'1', N'1903486709602062336', N'2026-07-22 15:06:16.533', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079825038730727424', N'2079809778514792448', NULL, N'ProcessLeaveCancell', N'3', N'1903486709602062336', N'2026-07-22 15:06:36.893', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079825705708949504', N'2079807667064410112', N'2079808600624205824', NULL, N'1', N'1903486709602062336', N'2026-07-22 15:08:37.277', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079825705708949504', N'2079808600624205824', N'2079809778514792448', NULL, N'2', N'1903486709602062336', N'2026-07-22 15:09:01.417', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079823631856308224', N'2079807667064410112', N'2079808600624205824', NULL, N'1', N'1903486709602062336', N'2026-07-22 15:10:03.273', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079826769581576192', N'2079807667064410112', N'2079808724595249152', NULL, N'1', N'1903486709602062336', N'2026-07-22 15:18:57.377', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079826769581576192', N'2079808724595249152', N'2079808873866334208', NULL, N'2', N'1903486709602062336', N'2026-07-22 15:22:04.847', N'1903486709602062336', N'2026-07-22 15:23:31.780')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079826769581576192', N'2079809906873077760', NULL, N'ProcessLeaveCancell', N'4', N'1903486709602062336', N'2026-07-22 15:24:24.297', N'1903486709602062336', N'2026-07-22 15:24:36.867')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079827173505634304', N'2079807667064410112', N'2079809045400784896', NULL, N'1', N'1903486709602062336', N'2026-07-22 15:26:04.760', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079830880456675328', N'2079807667064410112', N'2079809245062238208', NULL, N'1', N'1903486709602062336', N'2026-07-22 15:30:00.637', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079830880456675328', N'2079809399479734272', N'2079809544439074816', NULL, N'3', N'1903486709602062336', N'2026-07-22 15:31:09.877', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079831015152553984', N'2079809399479734272', N'2079809544439074816', NULL, N'2', N'1903486709602062336', N'2026-07-22 16:08:59.110', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079831015152553984', N'2079809544439074816', NULL, N'ProcessLeaveCancell', N'3', N'1903486709602062336', N'2026-07-22 16:09:09.260', N'1903486709602062336', N'2026-07-22 16:09:22.803')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079733578282962944', N'2009890853346217984', N'2036076248547069952', NULL, N'1', N'1903486709602062336', N'2026-07-22 09:03:36.407', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079827087866335232', N'2079809045400784896', N'2079809906873077760', NULL, N'3', N'1903486709602062336', N'2026-07-22 15:25:21.390', N'1903486709602062336', N'2026-07-22 15:25:26.770')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079827173505634304', N'2079809399479734272', NULL, N'ProcessLeaveCancell', N'4', N'1903486709602062336', N'2026-07-22 15:26:59.783', N'1903486709602062336', N'2026-07-22 15:27:03.557')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079830880456675328', N'2079809544439074816', NULL, N'ProcessLeaveCancell', N'4', N'1903486709602062336', N'2026-07-22 16:01:47.110', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079831015152553984', N'2079807667064410112', N'2079809399479734272', NULL, N'1', N'1903486709602062336', N'2026-07-22 16:08:42.937', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079733578282962944', N'2036076248547069952', N'2079393781261668352', NULL, N'2', N'1903486709602062336', N'2026-07-22 09:03:43.017', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079811193538744320', N'2079807978587951104', N'2079809778514792448', NULL, N'2', N'1903486709602062336', N'2026-07-22 14:11:33.550', N'1903486709602062336', N'2026-07-22 14:12:29.333')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079811193538744320', N'2079809906873077760', NULL, N'ProcessLeaveCancell', N'4', N'1903486709602062336', N'2026-07-22 14:13:03.517', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079823989164871680', N'2009890853346217984', N'2009892923604340736', NULL, N'1', N'1903486709602062336', N'2026-07-22 15:01:57.613', N'1903486709602062336', N'2026-07-22 15:59:44.963')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079823989164871680', N'2009892923604340736', N'2032353104544010240', NULL, N'2', N'1903486709602062336', N'2026-07-22 15:02:13.950', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079823989164871680', N'2032353104544010240', NULL, N'ProcessLeaveRequest', N'3', N'1903486709602062336', N'2026-07-22 15:02:47.743', N'1903486709602062336', N'2026-07-22 15:55:55.960')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079824160229560320', N'2009890853346217984', N'2009892923604340736', NULL, N'1', N'1903486709602062336', N'2026-07-22 15:03:14.853', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079825111464153088', N'2079807827882414080', N'2079809778514792448', NULL, N'2', N'1903486709602062336', N'2026-07-22 15:05:55.867', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079825038730727424', N'2079807827882414080', N'2079809778514792448', NULL, N'2', N'1903486709602062336', N'2026-07-22 15:06:26.273', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079825705708949504', N'2079809778514792448', N'2079809906873077760', NULL, N'3', N'1903486709602062336', N'2026-07-22 15:09:13.020', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079825705708949504', N'2079809906873077760', NULL, N'ProcessLeaveCancell', N'4', N'1903486709602062336', N'2026-07-22 15:09:30.847', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079823631856308224', N'2079808600624205824', N'2079808724595249152', NULL, N'2', N'1903486709602062336', N'2026-07-22 15:10:14.277', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079823631856308224', N'2079808724595249152', N'2079809906873077760', NULL, N'3', N'1903486709602062336', N'2026-07-22 15:10:21.733', N'1903486709602062336', N'2026-07-22 15:18:24.183')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079826769581576192', N'2079808873866334208', N'2079809906873077760', NULL, N'3', N'1903486709602062336', N'2026-07-22 15:24:14.210', N'1903486709602062336', N'2026-07-22 15:24:34.060')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079823631856308224', N'2079809906873077760', NULL, N'ProcessLeaveCancell', N'4', N'1903486709602062336', N'2026-07-22 15:10:48.793', N'1903486709602062336', N'2026-07-22 15:18:33.630')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079827087866335232', N'2079807667064410112', N'2079808873866334208', NULL, N'1', N'1903486709602062336', N'2026-07-22 15:24:53.463', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079827087866335232', N'2079808873866334208', N'2079809045400784896', NULL, N'2', N'1903486709602062336', N'2026-07-22 15:25:02.050', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079827087866335232', N'2079809906873077760', NULL, N'ProcessLeaveCancell', N'4', N'1903486709602062336', N'2026-07-22 15:25:36.063', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079827173505634304', N'2079809045400784896', N'2079809245062238208', NULL, N'2', N'1903486709602062336', N'2026-07-22 15:26:11.557', N'1903486709602062336', N'2026-07-22 15:26:48.817')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079827173505634304', N'2079809245062238208', N'2079809399479734272', NULL, N'3', N'1903486709602062336', N'2026-07-22 15:26:21.690', N'1903486709602062336', N'2026-07-22 15:26:51.643')
GO

INSERT INTO [Form].[WorkflowRuleStep] ([RuleId], [CurrentStepId], [NextStepId], [Guidance], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079830880456675328', N'2079809245062238208', N'2079809399479734272', NULL, N'2', N'1903486709602062336', N'2026-07-22 15:30:23.620', NULL, NULL)
GO


-- ----------------------------
-- Table structure for WorkflowStep
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Form].[WorkflowStep]') AND type IN ('U'))
	DROP TABLE [Form].[WorkflowStep]
GO

CREATE TABLE [Form].[WorkflowStep] (
  [StepId] bigint  NOT NULL,
  [FormTypeId] bigint  NULL,
  [StepNameCn] nvarchar(20) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NULL,
  [StepNameEn] nvarchar(50) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NULL,
  [IsStartStep] int  NULL,
  [Assignment] nvarchar(15) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NULL,
  [ReviewMode] nvarchar(15) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NULL,
  [IsReminderEnabled] int  NULL,
  [ReminderIntervalMinutes] int  NULL,
  [SortOrder] int  NULL,
  [CreatedBy] bigint  NULL,
  [CreatedDate] datetime2(3)  NULL,
  [ModifiedBy] bigint  NULL,
  [ModifiedDate] datetime2(3)  NULL
)
GO

ALTER TABLE [Form].[WorkflowStep] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'审批步骤Id',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStep',
'COLUMN', N'StepId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'表单类型Id',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStep',
'COLUMN', N'FormTypeId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'审批步骤名称（中文）',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStep',
'COLUMN', N'StepNameCn'
GO

EXEC sp_addextendedproperty
'MS_Description', N'审批步骤名称（英文）',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStep',
'COLUMN', N'StepNameEn'
GO

EXEC sp_addextendedproperty
'MS_Description', N'是否为开始步骤',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStep',
'COLUMN', N'IsStartStep'
GO

EXEC sp_addextendedproperty
'MS_Description', N'审批人选取方式（依部门人员组织架构、指定员工、自定义）',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStep',
'COLUMN', N'Assignment'
GO

EXEC sp_addextendedproperty
'MS_Description', N'签核方式（单签、会签）',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStep',
'COLUMN', N'ReviewMode'
GO

EXEC sp_addextendedproperty
'MS_Description', N'是否催签',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStep',
'COLUMN', N'IsReminderEnabled'
GO

EXEC sp_addextendedproperty
'MS_Description', N'催签间隔分钟',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStep',
'COLUMN', N'ReminderIntervalMinutes'
GO

EXEC sp_addextendedproperty
'MS_Description', N'排序',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStep',
'COLUMN', N'SortOrder'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStep',
'COLUMN', N'CreatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStep',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改人',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStep',
'COLUMN', N'ModifiedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'修改时间',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStep',
'COLUMN', N'ModifiedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'流程步骤表',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStep'
GO


-- ----------------------------
-- Records of WorkflowStep
-- ----------------------------
INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009890853346217984', N'1987217256446300160', N'发起人', N'Applicant', N'1', N'Org', N'Review', N'0', N'0', N'1', N'1903486709602062336', N'2026-01-10 15:31:41.000', N'1903486709602062336', N'2026-03-16 10:07:04.660')
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009892923604340736', N'1987217256446300160', N'组长', N'Team leader', N'0', N'Org', N'Review', N'1', N'1', N'2', N'1903486709602062336', N'2026-01-10 15:39:49.000', N'1903486709602062336', N'2026-06-24 19:47:55.820')
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009897830268932096', N'1987217256446300160', N'科长', N'Section Chief', N'0', N'Org', N'Review', N'1', N'1', N'3', N'1903486709602062336', N'2026-01-10 15:59:19.000', N'1903486709602062336', N'2026-06-24 19:47:49.373')
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2009898117243211776', N'1987217256446300160', N'经理', N'Manager', N'0', N'Org', N'Review', N'1', N'1', N'4', N'1903486709602062336', N'2026-01-10 16:00:27.000', N'1903486709602062336', N'2026-06-24 19:40:22.793')
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2029389483455156224', N'1987217256446300160', N'资深经理', N'Senior Manager', N'0', N'Org', N'Review', N'1', N'1', N'5', N'1903486709602062336', N'2026-03-05 10:52:11.747', N'1903486709602062336', N'2026-06-24 19:40:17.027')
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010240', N'1987217256446300160', N'人事专员', N'Human Specialist', N'0', N'DeptUser', N'Review', N'1', N'0', N'11', N'1903486709602062336', N'2026-03-13 15:08:34.077', N'1903486709602062336', N'2026-07-21 10:33:34.563')
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2032353104544010286', N'1987217256446300160', N'人事经理', N'Human  Manager', N'0', N'DeptUser', N'Review', N'1', N'0', N'12', N'1903486709602062336', N'2026-03-13 15:08:34.077', N'1903486709602062336', N'2026-07-21 10:33:41.757')
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2036076248547069952', N'1987217256446300160', N'厂长', N'Plant Director', N'0', N'Org', N'Review', N'0', N'0', N'6', N'1903486709602062336', N'2026-03-23 21:43:00.780', N'1903486709602062336', N'2026-06-24 19:40:08.750')
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079393781261668352', N'1987217256446300160', N'协理', N'Associate', N'0', N'Org', N'Review', N'0', N'0', N'7', N'1903486709602062336', N'2026-07-21 10:31:25.380', N'1903486709602062336', N'2026-07-21 10:31:56.943')
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394103849783296', N'1987217256446300160', N'副总', N'Vice President', N'0', N'Org', N'Review', N'0', N'0', N'8', N'1903486709602062336', N'2026-07-21 10:32:42.290', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394196795559936', N'1987217256446300160', N'总经理', N'General manager', N'0', N'Org', N'Review', N'0', N'0', N'9', N'1903486709602062336', N'2026-07-21 10:33:04.450', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079394286754992128', N'1987217256446300160', N'董事长', N'Chairman', N'0', N'Org', N'Review', N'0', N'0', N'10', N'1903486709602062336', N'2026-07-21 10:33:25.897', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807667064410112', N'2074764225741459456', N'发起人', N'Applicant', N'1', N'Org', N'Review', N'0', N'0', N'1', N'1903486709602062336', N'2026-07-22 13:56:03.443', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807827882414080', N'2074764225741459456', N'组长', N'Team leader', N'0', N'Org', N'Review', N'0', N'0', N'1', N'1903486709602062336', N'2026-07-22 13:56:41.787', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079807978587951104', N'2074764225741459456', N'科长', N'Section Chief', N'0', N'Org', N'Review', N'0', N'0', N'3', N'1903486709602062336', N'2026-07-22 13:57:17.717', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808600624205824', N'2074764225741459456', N'经理', N'Manager', N'0', N'Org', N'Review', N'0', N'0', N'4', N'1903486709602062336', N'2026-07-22 13:59:46.020', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808724595249152', N'2074764225741459456', N'资深经理', N'Senior Manager', N'0', N'Org', N'Review', N'0', N'0', N'5', N'1903486709602062336', N'2026-07-22 14:00:15.580', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079808873866334208', N'2074764225741459456', N'厂长', N'Plant Director', N'0', N'Org', N'Review', N'0', N'0', N'6', N'1903486709602062336', N'2026-07-22 14:00:51.167', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809045400784896', N'2074764225741459456', N'协理', N'Associate', N'0', N'Org', N'Review', N'0', N'0', N'7', N'1903486709602062336', N'2026-07-22 14:01:32.063', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809245062238208', N'2074764225741459456', N'副总', N'Vice President', N'0', N'Org', N'Review', N'0', N'0', N'8', N'1903486709602062336', N'2026-07-22 14:02:19.667', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809399479734272', N'2074764225741459456', N'总经理', N'General manager', N'0', N'Org', N'Review', N'0', N'0', N'9', N'1903486709602062336', N'2026-07-22 14:02:56.483', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809544439074816', N'2074764225741459456', N'董事长', N'Chairman', N'0', N'Org', N'Review', N'0', N'0', N'10', N'1903486709602062336', N'2026-07-22 14:03:31.043', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809778514792448', N'2074764225741459456', N'人事专员', N'Human Specialist', N'0', N'DeptUser', N'Review', N'0', N'0', N'11', N'1903486709602062336', N'2026-07-22 14:04:26.853', NULL, NULL)
GO

INSERT INTO [Form].[WorkflowStep] ([StepId], [FormTypeId], [StepNameCn], [StepNameEn], [IsStartStep], [Assignment], [ReviewMode], [IsReminderEnabled], [ReminderIntervalMinutes], [SortOrder], [CreatedBy], [CreatedDate], [ModifiedBy], [ModifiedDate]) VALUES (N'2079809906873077760', N'2074764225741459456', N'人事经理', N'Human Manager', N'0', N'DeptUser', N'Review', N'0', N'0', N'12', N'1903486709602062336', N'2026-07-22 14:04:57.457', N'1903486709602062336', N'2026-07-23 16:45:03.133')
GO


-- ----------------------------
-- Table structure for WorkflowStepCustom
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Form].[WorkflowStepCustom]') AND type IN ('U'))
	DROP TABLE [Form].[WorkflowStepCustom]
GO

CREATE TABLE [Form].[WorkflowStepCustom] (
  [StepId] bigint  NOT NULL,
  [Guidance] nvarchar(30) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [LogicalExplanation] nvarchar(150) COLLATE Chinese_PRC_90_CI_AS_SC_UTF8  NOT NULL,
  [CreatedBy] bigint  NOT NULL,
  [CreatedDate] datetime2(3)  NOT NULL
)
GO

ALTER TABLE [Form].[WorkflowStepCustom] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'审批步骤Id',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepCustom',
'COLUMN', N'StepId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'导向',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepCustom',
'COLUMN', N'Guidance'
GO

EXEC sp_addextendedproperty
'MS_Description', N'逻辑说明',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepCustom',
'COLUMN', N'LogicalExplanation'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepCustom',
'COLUMN', N'CreatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepCustom',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'步骤人员来源表-依照自定义',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepCustom'
GO


-- ----------------------------
-- Records of WorkflowStepCustom
-- ----------------------------

-- ----------------------------
-- Table structure for WorkflowStepDeptUser
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Form].[WorkflowStepDeptUser]') AND type IN ('U'))
	DROP TABLE [Form].[WorkflowStepDeptUser]
GO

CREATE TABLE [Form].[WorkflowStepDeptUser] (
  [StepId] bigint  NOT NULL,
  [DepartmentId] bigint  NOT NULL,
  [PositionId] bigint  NOT NULL,
  [CreatedBy] bigint  NOT NULL,
  [CreatedDate] datetime2(3)  NOT NULL
)
GO

ALTER TABLE [Form].[WorkflowStepDeptUser] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'审批步骤Id',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepDeptUser',
'COLUMN', N'StepId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'部门Id',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepDeptUser',
'COLUMN', N'DepartmentId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'员工职级Id',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepDeptUser',
'COLUMN', N'PositionId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepDeptUser',
'COLUMN', N'CreatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepDeptUser',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'步骤人员来源表-依照指定部门员工职级职业',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepDeptUser'
GO


-- ----------------------------
-- Records of WorkflowStepDeptUser
-- ----------------------------
INSERT INTO [Form].[WorkflowStepDeptUser] ([StepId], [DepartmentId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2032353104544010240', N'1950000000000000156', N'1351602631784529920', N'1903486709602062336', N'2026-07-21 10:33:34.827')
GO

INSERT INTO [Form].[WorkflowStepDeptUser] ([StepId], [DepartmentId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2032353104544010286', N'1950000000000000156', N'1351585319710883840', N'1903486709602062336', N'2026-07-21 10:33:42.000')
GO

INSERT INTO [Form].[WorkflowStepDeptUser] ([StepId], [DepartmentId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2079809778514792448', N'1950000000000000156', N'1351602631784529920', N'1903486709602062336', N'2026-07-22 14:04:26.857')
GO

INSERT INTO [Form].[WorkflowStepDeptUser] ([StepId], [DepartmentId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2079809906873077760', N'1950000000000000156', N'1351585319710883840', N'1903486709602062336', N'2026-07-23 16:45:03.170')
GO


-- ----------------------------
-- Table structure for WorkflowStepOrg
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Form].[WorkflowStepOrg]') AND type IN ('U'))
	DROP TABLE [Form].[WorkflowStepOrg]
GO

CREATE TABLE [Form].[WorkflowStepOrg] (
  [StepId] bigint  NOT NULL,
  [DeptLeaveId] bigint  NOT NULL,
  [PositionId] bigint  NOT NULL,
  [CreatedBy] bigint  NOT NULL,
  [CreatedDate] datetime2(3)  NOT NULL
)
GO

ALTER TABLE [Form].[WorkflowStepOrg] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'审批步骤Id',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepOrg',
'COLUMN', N'StepId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'部门级别Id',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepOrg',
'COLUMN', N'DeptLeaveId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'员工职级Id',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepOrg',
'COLUMN', N'PositionId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepOrg',
'COLUMN', N'CreatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepOrg',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'步骤人员来源表-依照组织架构',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepOrg'
GO


-- ----------------------------
-- Records of WorkflowStepOrg
-- ----------------------------
INSERT INTO [Form].[WorkflowStepOrg] ([StepId], [DeptLeaveId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2009892923604340736', N'1949169142347206656', N'1351602631784529920', N'1903486709602062336', N'2026-06-24 19:47:55.827')
GO

INSERT INTO [Form].[WorkflowStepOrg] ([StepId], [DeptLeaveId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2009897830268932096', N'1949168956883472384', N'1351600746193223680', N'1903486709602062336', N'2026-06-24 19:47:49.383')
GO

INSERT INTO [Form].[WorkflowStepOrg] ([StepId], [DeptLeaveId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2009898117243211776', N'1949167957770899456', N'1351585319710883840', N'1903486709602062336', N'2026-06-24 19:40:22.800')
GO

INSERT INTO [Form].[WorkflowStepOrg] ([StepId], [DeptLeaveId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2029389483455156224', N'1949167957770899456', N'1351584156689104896', N'1903486709602062336', N'2026-06-24 19:40:17.037')
GO

INSERT INTO [Form].[WorkflowStepOrg] ([StepId], [DeptLeaveId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2036076248547069952', N'1351405026328707072', N'1351584014896463872', N'1903486709602062336', N'2026-06-24 19:40:08.800')
GO

INSERT INTO [Form].[WorkflowStepOrg] ([StepId], [DeptLeaveId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2079393781261668352', N'1351403528752463872', N'1351583636813512704', N'1903486709602062336', N'2026-07-21 10:31:57.277')
GO

INSERT INTO [Form].[WorkflowStepOrg] ([StepId], [DeptLeaveId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2079394103849783296', N'1350917348311171072', N'1351583500196642816', N'1903486709602062336', N'2026-07-21 10:32:42.373')
GO

INSERT INTO [Form].[WorkflowStepOrg] ([StepId], [DeptLeaveId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2079394196795559936', N'1350917348311171072', N'1351582085961220096', N'1903486709602062336', N'2026-07-21 10:33:04.537')
GO

INSERT INTO [Form].[WorkflowStepOrg] ([StepId], [DeptLeaveId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2079394286754992128', N'1350917348311171072', N'1351581732096180224', N'1903486709602062336', N'2026-07-21 10:33:25.980')
GO

INSERT INTO [Form].[WorkflowStepOrg] ([StepId], [DeptLeaveId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2079807827882414080', N'1949169142347206656', N'1351602631784529920', N'1903486709602062336', N'2026-07-22 13:56:41.790')
GO

INSERT INTO [Form].[WorkflowStepOrg] ([StepId], [DeptLeaveId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2079807978587951104', N'1949168956883472384', N'1351600746193223680', N'1903486709602062336', N'2026-07-22 13:57:17.720')
GO

INSERT INTO [Form].[WorkflowStepOrg] ([StepId], [DeptLeaveId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2079808600624205824', N'1949167957770899456', N'1351585319710883840', N'1903486709602062336', N'2026-07-22 13:59:46.027')
GO

INSERT INTO [Form].[WorkflowStepOrg] ([StepId], [DeptLeaveId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2079808724595249152', N'1949167957770899456', N'1351584156689104896', N'1903486709602062336', N'2026-07-22 14:00:15.580')
GO

INSERT INTO [Form].[WorkflowStepOrg] ([StepId], [DeptLeaveId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2079808873866334208', N'1351405026328707072', N'1351584014896463872', N'1903486709602062336', N'2026-07-22 14:00:51.173')
GO

INSERT INTO [Form].[WorkflowStepOrg] ([StepId], [DeptLeaveId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2079809045400784896', N'1351403528752463872', N'1351583636813512704', N'1903486709602062336', N'2026-07-22 14:01:32.067')
GO

INSERT INTO [Form].[WorkflowStepOrg] ([StepId], [DeptLeaveId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2079809245062238208', N'1350917348311171072', N'1351583500196642816', N'1903486709602062336', N'2026-07-22 14:02:19.673')
GO

INSERT INTO [Form].[WorkflowStepOrg] ([StepId], [DeptLeaveId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2079809399479734272', N'1351403528752463872', N'1351582085961220096', N'1903486709602062336', N'2026-07-22 14:02:56.487')
GO

INSERT INTO [Form].[WorkflowStepOrg] ([StepId], [DeptLeaveId], [PositionId], [CreatedBy], [CreatedDate]) VALUES (N'2079809544439074816', N'1350917348311171072', N'1351581732096180224', N'1903486709602062336', N'2026-07-22 14:03:31.050')
GO


-- ----------------------------
-- Table structure for WorkflowStepUser
-- ----------------------------
IF EXISTS (SELECT * FROM sys.all_objects WHERE object_id = OBJECT_ID(N'[Form].[WorkflowStepUser]') AND type IN ('U'))
	DROP TABLE [Form].[WorkflowStepUser]
GO

CREATE TABLE [Form].[WorkflowStepUser] (
  [StepId] bigint  NOT NULL,
  [DepartmentId] bigint  NOT NULL,
  [UserId] bigint  NOT NULL,
  [CreatedBy] bigint  NOT NULL,
  [CreatedDate] datetime2(3)  NOT NULL
)
GO

ALTER TABLE [Form].[WorkflowStepUser] SET (LOCK_ESCALATION = TABLE)
GO

EXEC sp_addextendedproperty
'MS_Description', N'步骤Id',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepUser',
'COLUMN', N'StepId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'部门Id',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepUser',
'COLUMN', N'DepartmentId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'员工Id',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepUser',
'COLUMN', N'UserId'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建人',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepUser',
'COLUMN', N'CreatedBy'
GO

EXEC sp_addextendedproperty
'MS_Description', N'创建时间',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepUser',
'COLUMN', N'CreatedDate'
GO

EXEC sp_addextendedproperty
'MS_Description', N'步骤人员来源表-依照指定员工',
'SCHEMA', N'Form',
'TABLE', N'WorkflowStepUser'
GO


-- ----------------------------
-- Records of WorkflowStepUser
-- ----------------------------

-- ----------------------------
-- Primary Key structure for table ControlInfo
-- ----------------------------
ALTER TABLE [Form].[ControlInfo] ADD CONSTRAINT [PK__FormCont__3399DDEB13F7B41D] PRIMARY KEY CLUSTERED ([ControlCode])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table FormAttachment
-- ----------------------------
ALTER TABLE [Form].[FormAttachment] ADD CONSTRAINT [PK__FormFile__6F0F98BF2AF1086B] PRIMARY KEY CLUSTERED ([AttachmentId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table FormGroup
-- ----------------------------
ALTER TABLE [Form].[FormGroup] ADD CONSTRAINT [PK__FormClas__0201AC43C1A619E2] PRIMARY KEY CLUSTERED ([FormGroupId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table FormInstance
-- ----------------------------
ALTER TABLE [Form].[FormInstance] ADD CONSTRAINT [PK__FormInfo__FB05B7DDADCFEA72] PRIMARY KEY CLUSTERED ([FormId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table FormType
-- ----------------------------
ALTER TABLE [Form].[FormType] ADD CONSTRAINT [PK__FormType__902E30B3CC4BDD80] PRIMARY KEY CLUSTERED ([FormTypeId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table FormTypeField
-- ----------------------------
ALTER TABLE [Form].[FormTypeField] ADD CONSTRAINT [PK__FormFiel__C8B6FF07B40B64B3] PRIMARY KEY CLUSTERED ([FieldId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table LeaveCancell
-- ----------------------------
ALTER TABLE [Form].[LeaveCancell] ADD CONSTRAINT [PK__LeaveCan__FB05B7DD369322D1] PRIMARY KEY CLUSTERED ([FormId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table LeaveRequest
-- ----------------------------
ALTER TABLE [Form].[LeaveRequest] ADD CONSTRAINT [PK__LeaveIns__796DB959B422B703] PRIMARY KEY CLUSTERED ([FormId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table WorkflowRule
-- ----------------------------
ALTER TABLE [Form].[WorkflowRule] ADD CONSTRAINT [PK__Workflow__110458E2803405C7] PRIMARY KEY CLUSTERED ([RuleId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table WorkflowStep
-- ----------------------------
ALTER TABLE [Form].[WorkflowStep] ADD CONSTRAINT [PK__FormAppr__243433573D582151] PRIMARY KEY CLUSTERED ([StepId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table WorkflowStepCustom
-- ----------------------------
ALTER TABLE [Form].[WorkflowStepCustom] ADD CONSTRAINT [PK__FormStep__0A0392F2D13B9AA7] PRIMARY KEY CLUSTERED ([StepId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table WorkflowStepDeptUser
-- ----------------------------
ALTER TABLE [Form].[WorkflowStepDeptUser] ADD CONSTRAINT [PK__Workflow__24343357AD16EE5C] PRIMARY KEY CLUSTERED ([StepId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table WorkflowStepOrg
-- ----------------------------
ALTER TABLE [Form].[WorkflowStepOrg] ADD CONSTRAINT [PK__Workflow__243433574BF4D0F4] PRIMARY KEY CLUSTERED ([StepId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO


-- ----------------------------
-- Primary Key structure for table WorkflowStepUser
-- ----------------------------
ALTER TABLE [Form].[WorkflowStepUser] ADD CONSTRAINT [PK__FormStep__8AB6E1C4820E75E2] PRIMARY KEY CLUSTERED ([StepId])
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)  
ON [PRIMARY]
GO

