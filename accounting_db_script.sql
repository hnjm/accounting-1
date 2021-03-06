USE [accounting]
GO
/****** Object:  Table [dbo].[internal_payment]    Script Date: 2/2/2018 1:59:05 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[internal_payment](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[date] [datetime] NULL,
	[payment_number] [nchar](31) NULL,
	[name] [nchar](31) NULL,
	[content] [nchar](31) NULL,
	[group_name] [nchar](31) NULL,
	[advance_payment] [bigint] NULL,
	[reimbursement] [bigint] NULL,
	[actually_spent] [bigint] NULL,
	[note] [ntext] NULL,
	[addr] [nchar](63) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[v_internal_payment]    Script Date: 2/2/2018 1:59:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[v_internal_payment]
AS
SELECT        DATEPART(YEAR, date) AS year, DATEPART(QUARTER, date) AS qtr, group_name, advance_payment / 1000 AS advance_payment, reimbursement / 1000 AS reimbursement, 
                         actually_spent / 1000 AS actually_spent
FROM            dbo.internal_payment
WHERE        (DATEDIFF(yyyy, date, GETDATE()) < 5)

GO
/****** Object:  Table [dbo].[external_payment]    Script Date: 2/2/2018 1:59:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[external_payment](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[date] [datetime] NULL,
	[payment_number] [nchar](31) NULL,
	[name] [nchar](31) NULL,
	[content] [ntext] NULL,
	[building] [nchar](31) NULL,
	[group_name] [nchar](31) NULL,
	[spent] [bigint] NULL,
	[note] [ntext] NULL,
	[addr] [nchar](63) NULL,
	[constr_org] [nchar](31) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[v_external_payment]    Script Date: 2/2/2018 1:59:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[v_external_payment]
AS
SELECT        group_name, spent / 1000 AS spent, DATEPART(year, date) AS year, DATEPART(quarter, date) AS qtr
FROM            dbo.external_payment
WHERE        (DATEDIFF(yyyy, date, GETDATE()) < 5)

GO
/****** Object:  Table [dbo].[salary]    Script Date: 2/2/2018 1:59:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[salary](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[month] [bigint] NULL,
	[date] [datetime] NULL,
	[payment_number] [nchar](31) NULL,
	[name] [nchar](31) NULL,
	[group_name] [nchar](31) NULL,
	[content] [ntext] NULL,
	[salary] [bigint] NULL,
	[note] [ntext] NULL,
	[addr] [nchar](63) NULL,
	[bsalary] [bigint] NULL,
	[esalary] [bigint] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[v_salary]    Script Date: 2/2/2018 1:59:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[v_salary]
AS
SELECT        group_name, salary / 1000 AS salary, DATEPART(year, date) AS year, DATEPART(quarter, date) AS qtr
FROM            dbo.salary
WHERE        (DATEDIFF(yyyy, date, GETDATE()) < 5)

GO
/****** Object:  Table [dbo].[receipts]    Script Date: 2/2/2018 1:59:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[receipts](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[date] [datetime] NULL,
	[receipt_number] [nchar](31) NULL,
	[name] [nchar](31) NULL,
	[content] [ntext] NULL,
	[amount] [bigint] NULL,
	[note] [ntext] NULL,
	[addr] [nchar](63) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[v_months]    Script Date: 2/2/2018 1:59:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[v_months]
AS
SELECT        t1.date AS month, t4.receipt, t1.inter_pay, t2.exter_pay, t3.salary, t4.receipt - t1.inter_pay - t2.exter_pay - t3.salary AS remain
FROM            (SELECT        date, SUM(actually_spent) AS inter_pay
                          FROM            dbo.internal_payment
                          WHERE        (date BETWEEN '2016-12-01' AND '2016-12-02')
                          GROUP BY date) AS t1 INNER JOIN
                             (SELECT        date, SUM(spent) AS exter_pay
                               FROM            dbo.external_payment
                               WHERE        (date BETWEEN '2016-12-01' AND '2016-12-02')
                               GROUP BY date) AS t2 ON t1.date = t2.date INNER JOIN
                             (SELECT        date, SUM(salary) AS salary
                               FROM            dbo.salary
                               WHERE        (date BETWEEN '2016-12-01' AND '2016-12-02')
                               GROUP BY date) AS t3 ON t1.date = t3.date INNER JOIN
                             (SELECT        date, SUM(amount) AS receipt
                               FROM            dbo.receipts
                               WHERE        (date BETWEEN '2016-12-01' AND '2016-12-02')
                               GROUP BY date) AS t4 ON t1.date = t4.date

GO
/****** Object:  View [dbo].[v_days]    Script Date: 2/2/2018 1:59:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[v_days]
AS
SELECT        group_name, date, name, actually_spent AS inter_pay, 0 AS exter_pay, 0 AS salary
FROM            internal_payment
WHERE        date BETWEEN '2016-12-01' AND '2016-12-02'
UNION
SELECT        group_name, date, name, 0 AS inter_pay, spent AS exter_pay, 0 AS salary
FROM            external_payment
WHERE        date BETWEEN '2016-12-01' AND '2016-12-02'

GO
/****** Object:  View [dbo].[v_day_sum]    Script Date: 2/2/2018 1:59:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[v_day_sum]
AS
SELECT        date, SUM(receipt) AS receipt, SUM(interpay) AS interpay, SUM(exterpay) AS exterpay, SUM(salary) AS salary, SUM(receipt) - SUM(interpay) - SUM(exterpay) - SUM(salary) AS sum
FROM            (SELECT        date, amount AS receipt, 0 AS interpay, 0 AS exterpay, 0 AS salary
                          FROM            dbo.receipts
                          UNION
                          SELECT        date, 0 AS receipt, advance_payment - reimbursement AS interpay, 0 AS exterpay, 0 AS salary
                          FROM            dbo.internal_payment
                          UNION
                          SELECT        date, 0 AS receipt, 0 AS interpay, spent AS exterpay, 0 AS salary
                          FROM            dbo.external_payment
                          UNION
                          SELECT        date, 0 AS receipt, 0 AS interpay, 0 AS exterpay, salary
                          FROM            dbo.salary) AS t1
GROUP BY date
GO
/****** Object:  Table [dbo].[building]    Script Date: 2/2/2018 1:59:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[building](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[name] [nchar](31) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[constr_org]    Script Date: 2/2/2018 1:59:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[constr_org](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[name] [nchar](31) NULL,
 CONSTRAINT [PK_constr_org] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[group_name]    Script Date: 2/2/2018 1:59:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[group_name](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[name] [nchar](31) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Query]    Script Date: 2/2/2018 1:59:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Query](
	[id] [float] NULL,
	[date] [datetime] NULL,
	[payment_number] [nvarchar](255) NULL,
	[name] [nvarchar](255) NULL,
	[content] [nvarchar](255) NULL,
	[group_name] [nvarchar](255) NULL,
	[spent] [float] NULL,
	[note] [nvarchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[receipts_content]    Script Date: 2/2/2018 1:59:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[receipts_content](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[content] [ntext] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[rpt_days]    Script Date: 2/2/2018 1:59:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--select group_name, date, name, actually_spent as inter_pay, 0 as exter_pay, 0 as salary
--from internal_payment where date between '2016-12-01' and '2016-12-30'
--union
--select group_name, date, name, 0 as inter_pay, spent as exter_pay, 0 as salary
--from external_payment where date between '2016-12-01' and '2016-12-02'
--union
--select group_name, date, name, 0 as inter_pay, 0 as exter_pay, salary
--from salary where date between '2016-12-01' and '2016-12-02'

CREATE PROCEDURE [dbo].[rpt_days]
    @startDate date,   
    @endDate date   
AS   
		select group_name, [date], [name], sum(inter_pay) as inter_pay, sum(exter_pay) as exter_pay, sum(salary) as salary
	from
	 (select group_name, [date], [name], actually_spent as inter_pay, 0 as exter_pay, 0 as salary
		from internal_payment where [date] between @startDate and @endDate
	 union
	 select group_name, [date], [name], 0 as inter_pay, spent as exter_pay, 0 as salary
		from external_payment where [date] between @startDate and @endDate
	 union
		select group_name, [date], [name], 0 as inter_pay, 0 as exter_pay, salary
	 from salary where [date] between @startDate and @endDate
	 ) as t1
	 group by group_name, [date], [name]

GO
/****** Object:  StoredProcedure [dbo].[rpt_months]    Script Date: 2/2/2018 1:59:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--select group_name, date, name, actually_spent as inter_pay, 0 as exter_pay, 0 as salary
--from internal_payment where date between '2016-12-01' and '2016-12-30'
--union
--select group_name, date, name, 0 as inter_pay, spent as exter_pay, 0 as salary
--from external_payment where date between '2016-12-01' and '2016-12-02'
--union
--select group_name, date, name, 0 as inter_pay, 0 as exter_pay, salary
--from salary where date between '2016-12-01' and '2016-12-02'

CREATE PROCEDURE [dbo].[rpt_months]
    @startDate date,   
    @endDate date   
AS   
	select group_name, month as date, '' as name,
                sum(inter_pay) as inter_pay, sum(exter_pay) as exter_pay, sum(salary) as salary
                from
                (select group_name, FORMAT(date, 'yyyy-MM') as month, actually_spent as inter_pay, 0 as exter_pay, 0 as salary
                from internal_payment where date between @startDate and @endDate
                union
                select group_name, FORMAT(date, 'yyyy-MM') as month, 0 as inter_pay, spent as exter_pay, 0 as salary
                from external_payment where date between @startDate and @endDate
                union
                select group_name, FORMAT(date, 'yyyy-MM') as month, 0 as inter_pay, 0 as exter_pay, salary
                from salary where date between @startDate and @endDate) as t
                group by group_name, month

GO
/****** Object:  StoredProcedure [dbo].[rpt_weeks]    Script Date: 2/2/2018 1:59:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--select group_name, date, name, actually_spent as inter_pay, 0 as exter_pay, 0 as salary
--from internal_payment where date between '2016-12-01' and '2016-12-30'
--union
--select group_name, date, name, 0 as inter_pay, spent as exter_pay, 0 as salary
--from external_payment where date between '2016-12-01' and '2016-12-02'
--union
--select group_name, date, name, 0 as inter_pay, 0 as exter_pay, salary
--from salary where date between '2016-12-01' and '2016-12-02'

CREATE PROCEDURE [dbo].[rpt_weeks]
    @startDate date,   
    @endDate date   
AS   
	select group_name, week as date, '' as name,
 sum(inter_pay) as inter_pay, sum(exter_pay) as exter_pay, sum(salary) as salary
 from
 (select group_name, FORMAT(date, 'yyyy') +'-'+ cast(datepart(WEEK, date) as nchar) as week, actually_spent as inter_pay, 0 as exter_pay, 0 as salary
 from internal_payment where date between @startDate and @endDate
 union
 select group_name, FORMAT(date, 'yyyy') +'-'+ cast(datepart(WEEK, date) as nchar) as week, 0 as inter_pay, spent as exter_pay, 0 as salary
 from external_payment where date between @startDate and @endDate
 union
 select group_name, FORMAT(date, 'yyyy') +'-'+ cast(datepart(WEEK, date) as nchar) as week, 0 as inter_pay, 0 as exter_pay, salary
 from salary where date between @startDate and @endDate) as t1
 group by group_name, week

GO
/****** Object:  StoredProcedure [dbo].[sta_month]    Script Date: 2/2/2018 1:59:06 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--select group_name, date, name, actually_spent as inter_pay, 0 as exter_pay, 0 as salary
--from internal_payment where date between '2016-12-01' and '2016-12-30'
--union
--select group_name, date, name, 0 as inter_pay, spent as exter_pay, 0 as salary
--from external_payment where date between '2016-12-01' and '2016-12-02'
--union
--select group_name, date, name, 0 as inter_pay, 0 as exter_pay, salary
--from salary where date between '2016-12-01' and '2016-12-02'

CREATE PROCEDURE [dbo].[sta_month]
    @startDate date,   
    @endDate date   
AS   
	select month, sum(receipt) as receipt, sum(inter_pay) as inter_pay, sum(exter_pay) as exter_pay, sum(salary) as salary, 0 as remain 
	from(
	  select convert(nvarchar(7), date, 120) as month, 0 as receipt, sum(actually_spent) as inter_pay, 0 as exter_pay, 0 as salary
		from internal_payment where ([date] between @startDate and @endDate)
		group by convert(nvarchar(7), date, 120)
	  union
		  select convert(nvarchar(7), date, 120) as month, 0 as receipt, 0 as inter_pay, sum(spent) as exter_pay, 0 as salary
		  from external_payment where [date] between @startDate and @endDate
		  group by convert(nvarchar(7), date, 120)
	  union
		  select convert(nvarchar(7), date, 120) as month, 0 as receipt, 0 as inter_pay, 0 as exter_pay, sum(salary) as salary
		  from salary where [date] between @startDate and @endDate
		  group by convert(nvarchar(7), date, 120)
	  union
		  select convert(nvarchar(7), date, 120) as month, sum(amount) as receipt, 0 as inter_pay, 0 as exter_pay, 0 as salary
		  from receipts where [date] between @startDate and @endDate
		  group by convert(nvarchar(7), date, 120)
	) as t
	group by month

GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "t1"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 174
               Right = 300
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 12
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_day_sum'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_day_sum'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_days'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_days'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "external_payment"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 204
               Right = 294
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_external_payment'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_external_payment'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4[30] 2[40] 3) )"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "internal_payment"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 223
            End
            DisplayFlags = 280
            TopColumn = 6
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_internal_payment'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_internal_payment'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "t1"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 102
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "t2"
            Begin Extent = 
               Top = 6
               Left = 246
               Bottom = 102
               Right = 416
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "t3"
            Begin Extent = 
               Top = 102
               Left = 38
               Bottom = 198
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "t4"
            Begin Extent = 
               Top = 102
               Left = 246
               Bottom = 198
               Right = 416
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_months'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_months'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "salary"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 221
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_salary'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'v_salary'
GO
