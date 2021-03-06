/***************************按星期查询数据创建存储**************/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,jzheng,Name>
-- Create date: <Create Date,9/16/2011,>
-- Description:	<Description,按周获取前三个月付费总数（包括快速支付的费用和Online的支付费用）,>
-- =============================================
CREATE PROCEDURE [dbo].[GetPaymentAmount]
	-- Add the parameters for the stored procedure here
	@merchantID UNIQUEIDENTIFIER ='00000000-0000-0000-0000-000000000000'
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Insert statements for procedure here
	if @merchantID='00000000-0000-0000-0000-000000000000'
		begin
			select sum(a.TotalDue) as TotalDue,a.week from 
				(SELECT dbo.ConsumptionAccounts.PayAmount as TotalDue, DATENAME(WEEK, dbo.ConsumptionAccounts.CreatedDate) AS Week
				  FROM dbo.Merchant INNER JOIN
					  dbo.MerchantVSChargeItem ON dbo.Merchant.MerchantID = dbo.MerchantVSChargeItem.MerchantID INNER JOIN
					  dbo.MemberVSChargeItem ON dbo.MerchantVSChargeItem.MerchantVSChargeItemID = dbo.MemberVSChargeItem.MerchantVSChargeItemID INNER JOIN
					  dbo.ConsumptionAccounts ON dbo.MemberVSChargeItem.MemberVSChargeItemID = dbo.ConsumptionAccounts.MemberVSChargeItemID
						WHERE (dbo.ConsumptionAccounts.CreatedDate BETWEEN DATEADD(MM, - 3, GETDATE()) AND GETDATE()) 
							AND (dbo.ConsumptionAccounts.IsDeleted = 0) and (ISNULL(dbo.ConsumptionAccounts.Result,'False')='True')
							and (dbo.Merchant.MerchantID = @merchantID)
				 union ALL
				 SELECT dbo.FastpayAccounts.PayAmount AS TotalDue, DATENAME(WEEK, dbo.FastpayAccounts.CreatedDate) AS Week
					FROM dbo.MerchantVSChargeItem INNER JOIN
						  dbo.ChargeItem ON dbo.MerchantVSChargeItem.ChargeItemID = dbo.ChargeItem.ChargeItemID INNER JOIN
						  dbo.FastpayAccounts ON dbo.ChargeItem.ChargeItemID = dbo.FastpayAccounts.ChargeItemID
					WHERE (dbo.FastpayAccounts.CreatedDate BETWEEN DATEADD(MM, - 3, GETDATE()) AND GETDATE()) AND (dbo.FastpayAccounts.IsDeleted = 0) AND 
						  (ISNULL(dbo.FastpayAccounts.Result, 'False') = 'True'))
					  as a
			group by a.Week
		end
	else
		begin
			select sum(a.TotalDue) as TotalDue,a.week from 
				(SELECT dbo.ConsumptionAccounts.PayAmount as TotalDue, DATENAME(WEEK, dbo.ConsumptionAccounts.CreatedDate) AS Week
				  FROM dbo.Merchant INNER JOIN
					  dbo.MerchantVSChargeItem ON dbo.Merchant.MerchantID = dbo.MerchantVSChargeItem.MerchantID INNER JOIN
					  dbo.MemberVSChargeItem ON dbo.MerchantVSChargeItem.MerchantVSChargeItemID = dbo.MemberVSChargeItem.MerchantVSChargeItemID INNER JOIN
					  dbo.ConsumptionAccounts ON dbo.MemberVSChargeItem.MemberVSChargeItemID = dbo.ConsumptionAccounts.MemberVSChargeItemID
						WHERE (dbo.ConsumptionAccounts.CreatedDate BETWEEN DATEADD(MM, - 3, GETDATE()) AND GETDATE()) 
							AND (dbo.ConsumptionAccounts.IsDeleted = 0) and (ISNULL(dbo.ConsumptionAccounts.Result,'False')='True')
							
				 union ALL
				 SELECT dbo.FastpayAccounts.PayAmount AS TotalDue, DATENAME(WEEK, dbo.FastpayAccounts.CreatedDate) AS Week
					FROM dbo.MerchantVSChargeItem INNER JOIN
						  dbo.ChargeItem ON dbo.MerchantVSChargeItem.ChargeItemID = dbo.ChargeItem.ChargeItemID INNER JOIN
						  dbo.FastpayAccounts ON dbo.ChargeItem.ChargeItemID = dbo.FastpayAccounts.ChargeItemID
					WHERE (dbo.FastpayAccounts.CreatedDate BETWEEN DATEADD(MM, - 3, GETDATE()) AND GETDATE()) AND (dbo.FastpayAccounts.IsDeleted = 0) AND 
						  (ISNULL(dbo.FastpayAccounts.Result, 'False') = 'True'))
					  as a
			group by a.Week
		end
END
GO
	
select DATEADD(QUARTER, DATEDIFF(M, 0, GETDATE()), 0)


select COUNT(PayAmount) from ViewUserMerchantInfo as it where UserID='806858e6-034f-4de5-a57f-7a3089bfc480' 
	and it.CreatedDate between cast('9/10/2011' as DateTime) and cast('9/20/2011' as DateTime)
	
select * from Member where Email='121212@126.com'

select * from MemberVSService where MemberID='C045707E-A1C0-45E1-8889-135DCC388BDC'


select * from ViewMemberAndService where MemberID='C045707E-A1C0-45E1-8889-135DCC388BDC' 

select * from Services where ServiceID='A19FC1B1-E883-4A75-B556-8C16B13B92F8' 


select * from Merchant

select * from MemberTypeVSService where MemberTypeID='339E18D5-8BE8-668B-8623-41CF7A873DAA'

delete MerchantVSUser


SELECT * from CreditCardType


SELECT MVSC.MemberID, MVSC.AccountNumber, SUM(DA.Due) AS Due, SUM(DA.Due) - SUM(CA.PayAmount) AS Balance
FROM dbo.MemberVSChargeItem AS MVSC INNER JOIN
                          (SELECT MemberVSChargeItemID, IsDeleted, SUM(Due) AS Due
                            FROM dbo.DueAccounts
                            WHERE (ISNULL(IsDeleted, 0) = 0)
                            GROUP BY MemberVSChargeItemID, IsDeleted) AS DA 
                            ON DA.MemberVSChargeItemID = MVSC.MemberVSChargeItemID AND ISNULL(MVSC.IsDeleted, 0) = 0 AND ISNULL(DA.IsDeleted, 0) = 0 
									LEFT OUTER JOIN
									  (SELECT MemberVSChargeItemID, IsDeleted, SUM(PayAmount) AS PayAmount
										FROM dbo.ConsumptionAccounts
										WHERE (ISNULL(IsDeleted, 0) = 0)
										GROUP BY MemberVSChargeItemID, IsDeleted) AS CA 
										ON CA.MemberVSChargeItemID = MVSC.MemberVSChargeItemID AND ISNULL(MVSC.IsDeleted, 0) = 0 AND ISNULL(CA.IsDeleted, 0) = 0
GROUP BY MVSC.MemberID, MVSC.AccountNumber