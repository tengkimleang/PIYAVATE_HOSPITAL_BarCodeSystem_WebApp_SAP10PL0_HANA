USE [KOFIDB_COLTD_16102023]
GO
/****** Object:  StoredProcedure [dbo].[USP_CALLTRANSCATION_POSKOFI]    Script Date: 1/26/2024 4:29:44 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER PROC [dbo].[USP_CALLTRANSCATION_POSKOFI]
	@TYPE AS NVARCHAR(MAX),
	@par1 AS NVARCHAR(MAX),
	@par2 AS NVARCHAR(MAX),
	@par3 AS NVARCHAR(MAX),
	@par4 AS NVARCHAR(MAX),
	@par5 AS NVARCHAR(MAX)

AS
BEGIN
	SET NOCOUNT ON;
	--Start Purchase Order --
	IF @TYPE='GET_PURCHASE_ORDER'
	BEGIN
		SELECT 
				 DocNum AS DocNum
				,CardCode AS VendorCode
				,CardName AS VendorName
				,NumAtCard AS VendorRefNo
				,Format(DocDate,'dd-MMM-yyyy') AS PostingDate
				,DocTotal AS DocTotal 
				,DocEntry AS DocEntry
				
		FROM OPOR WHERE 
		Series=Case When @par1='' Then Series Else @par1 End AND  DocNum LIKE '%'+IIF(@par2='-1','',@par2)+'%' 
		And DocStatus='O' AND ISNULL(Cast(U_docDraft As nvarchar(max)),'')<>'oDraft'
		ORDER BY DocNum DESC
		OFFSET cast(@par3 as int) ROWS
		FETCH NEXT cast(@par4 as int) ROWS ONLY;
	END
	ELSE IF @TYPE='GET_Purchase_ORDER_Count'
	BEGIN
		SELECT 
				COUNT(DocNum) AS "Count" FROM OPOR WHERE 
				Series=Case When @par1='' Then Series Else @par1 End AND  DocNum LIKE '%'+IIF(@par2='-1','',@par2)+'%' 
				And DocStatus='O' AND ISNULL(Cast(U_docDraft As nvarchar(max)),'')<>'oDraft'
	END
	ELSE IF @TYPE='GET_Purchase_Order_Detail_By_DocNum'
	BEGIN
		SELECT 
			 A.CardCode AS VendorCode
			,A.CardName AS VendorName
			,C.Name AS ContactPerson
			,A.CntctCode AS ContactPersonId
			,A.NumAtCard AS VendorNo
			,A.DocNum AS Remarks
			,D.Name AS BranchName
			,ISNULL(A.Branch,1) AS BranchId
			,A.DocEntry AS BaseDocEntry
			,B.LineNum AS BaseLineNumber
			,B.ItemCode AS ItemCode
			,B.Dscription AS ItemName
			,IIF((B.OpenQty-ISNUll(B.U_oDraftQty,0))>0,(B.OpenQty-ISNUll(B.U_oDraftQty,0)),B.OpenQty) AS Qty
			,B.Price AS Price
			,B.LineTotal AS LineTotal
			,B.VatGroup AS VatCode
			,B.UomCode
			,B.WhsCode AS WarehouseCode
			,E.CodeBars AS BarCode
			,CASE WHEN E.ManBtchNum='Y' THEN
				'B'
			 WHEN E.ManSerNum='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS ManageItem
		FROM OPOR AS A
		LEFT JOIN POR1 AS B ON A.DocEntry=B.DocEntry
		LEFT JOIN OCPR AS C ON A.CardCode=C.CardCode AND A.CntctCode=C.CntctCode
		LEFT JOIN OUBR AS D ON D.Code=A.Branch
		LEFT JOIN OITM AS E ON E.ItemCode=B.ItemCode
		WHERE A.DocNum=@par2 And B.LineStatus='O' And ISNULL(B.U_oDraftStatus,'')<>'C'-- A.CardCode=@par1 
	END
	--End Purchase Order --
	--Start GoodReceipt PO --
	ELSE IF @TYPE='GET_GoodReceipt_PO'
	BEGIN
		SELECT 
				 DocNum AS DocNum
				,CardCode AS VendorCode
				,CardName AS VendorName
				,NumAtCard AS VendorRefNo
				,Format(DocDate,'dd-MMM-yyyy') AS PostingDate
				,DocTotal AS DocTotal FROM OPDN 
				WHERE Series=Case When @par1='' Then Series Else @par1 End AND  DocNum LIKE '%'+IIF(@par2='-1','',@par2)+'%'
				AND U_WebID<>'' And DocStatus='O'
				ORDER BY DocNum DESC
				OFFSET cast(@par3 as int) ROWS
				FETCH NEXT cast(@par4 as int) ROWS ONLY;
	END
	ELSE IF @TYPE='GET_Good_Reciept_Count'
	BEGIN
		SELECT 
				COUNT(DocNum) AS "Count" FROM OPDN WHERE 
				Series=Case When @par1='' Then Series Else @par1 End AND  DocNum LIKE '%'+IIF(@par2='-1','',@par2)+'%'
				AND U_WebID<>'' And DocStatus='O'
	END
	ELSE IF @TYPE='GET_GoodReceipt_PO_Detail_By_DocNum'
	BEGIN
		SELECT 
			A.DocNum AS DocNum
			,A.CardCode AS VendorCode
			,A.CardName AS VendorName
			,C.Name AS ContactPerson
			,A.CntctCode AS ContactPersonId
			,A.NumAtCard AS VendorNo
			,A.DocNum AS Remarks
			,A.BPLName AS BranchName
			,A.BPLId AS BranchId
			,A.DocEntry AS BaseDocEntry
			,B.LineNum AS BaseLineNumber
			,B.ItemCode AS ItemCode
			,B.Dscription AS ItemName
			,B.OpenQty AS Qty
			,B.Price AS Price
			,B.LineTotal AS LineTotal
			,B.TaxCode AS VatCode
			,B.WhsCode AS WarehouseCode
			,E.CodeBars AS BarCode
			,CASE WHEN E.ManBtchNum='Y' THEN
				'B'
			 WHEN E.ManSerNum='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS ManageItem
		FROM OPDN AS A
		LEFT JOIN PDN1 AS B ON A.DocEntry=B.DocEntry
		LEFT JOIN OCPR AS C ON A.CardCode=C.CardCode AND A.CntctCode=C.CntctCode
		LEFT JOIN OUBR AS D ON D.Code=A.Branch
		LEFT JOIN OITM AS E ON E.ItemCode=B.ItemCode
		WHERE A.CardCode=IIF(@par1='',A.CardCode,@par1) AND A.DocNum=@par2
		and B.LineStatus='O'
	END
	ELSE IF @TYPE='GetBatchSerialGoodReceipt'
	BEGIN
		--nt docNum, string itemCode, string itemType
		IF @par3='S'
		BEGIN
			SELECT 
				A.ItemCode AS ItemCode	
				,B.DistNumber AS SerialBatch
				,B.LotNumber AS LotNo
				,Format(B.ExpDate,'dd-MMM-yyyy') AS ExpDate
				,1 AS Qty
				,B.MnfSerial AS Mfr_Serial_No
				,B.MnfDate AS Mrf_Date

			FROM SRI1 AS A
			LEFT JOIN OSRN AS B ON A.ItemCode=B.ItemCode AND B.SysNumber=A.SysSerial
			LEFT JOIN OSRI AS C On C.ItemCode=A.ItemCode And C.SysSerial=A.SysSerial
			WHERE A.BaseNum=@par1 AND A.ItemCode=@par2 AND A.BaseType=20 And C.Status<>0
		END
		ELSE IF @par3='B'
		BEGIN
			SELECT 
				A.ItemCode AS ItemCode	
				,B.DistNumber AS SerialBatch
				,B.LotNumber AS LotNo
				,Format(ExpDate,'dd-MMM-yyyy') AS ExpDate
				,C.Quantity AS Qty
				,B.MnfSerial AS Mfr_Serial_No
				,B.MnfDate AS Mrf_Date

			FROM IBT1 AS A 
			LEFT JOIN OBTN AS B ON A.ItemCode=B.ItemCode AND A.BatchNum=B.DistNumber
			LEFT JOIN OBTQ AS C ON C.ItemCode=A.ItemCode and B.SysNumber=C.SysNumber
			WHERE A.BaseNum=@par1 AND A.ItemCode=@par2 AND A.BaseType=20
		END
	END
	ELSE IF @TYPE='BatchDetialGoodReceipt'
	BEGIN
	--int docNum, string serialBatch
			SELECT 
				 B.DistNumber AS SerialBatch
				,B.LotNumber AS LotNo
				,B.ExpDate AS ExpDate
				,B.Quantity AS Qty
			FROM SRI1 AS A
			LEFT JOIN OSRN AS B ON A.ItemCode=B.ItemCode AND B.SysNumber=A.SysSerial
			WHERE A.BaseNum=@par1 AND B.DistNumber=@par2 AND A.BaseType=20
			UNION ALL
			SELECT 
				 A.BatchNum
				,B.LotNumber
				,B.ExpDate
				,A.Quantity
			FROM IBT1 AS A 
			LEFT JOIN OBTN AS B ON A.ItemCode=B.ItemCode AND A.BatchNum=B.DistNumber
			WHERE A.BaseNum=@par1 AND B.DistNumber=@par2  AND A.BaseType=20
	END
	--End GoodReceipt PO --
	--Start Sale Order --
	ELSE IF @TYPE='GET_SALE_ORDER'
	BEGIN
		SELECT 
				 DocNum AS DocNum
				,CardCode AS VendorCode
				,CardName AS VendorName
				,NumAtCard AS VendorRefNo
				,Format(DocDate,'dd-MMM-yyyy') AS PostingDate
				,DocTotal AS DocTotal FROM ORDR WHERE 
				Series=Case When @par1='-1' Then Series Else @par1 End  AND DocNum LIKE '%'+@par2+'%' 
				AND DocStatus ='O'
				ORDER BY DocNum DESC
				OFFSET cast(@par3 as int) ROWS
				FETCH NEXT cast(@par4 as int) ROWS ONLY;
	END
	ELSE IF @TYPE='GET_SALE_ORDER_Count'
	BEGIN
		SELECT 
				COUNT(DocNum) AS "Count" FROM ORDR WHERE 
				Series=Case When @par1='-1' Then Series Else @par1 End  AND DocNum LIKE '%'+@par2+'%' 
				AND DocStatus ='O';
	END
	ELSE IF @TYPE='GET_Sale_Order_Detail_By_DocNum'
	BEGIN
		SELECT 
			 A.CardCode AS VendorCode
			,A.CardName AS VendorName
			,C.Name AS ContactPerson
			,ISNULL(A.CntctCode ,'')AS ContactPersonId
			,A.NumAtCard AS VendorNo
			,A.DocNum AS Remarks
			,A.BPLName AS BranchName
			,A.BPLId AS BranchId
			,A.DocEntry AS BaseDocEntry
			,B.LineNum AS BaseLineNumber
			,B.ItemCode AS ItemCode
			,B.Dscription AS ItemName
			,B.OpenQty AS Qty
			,B.Price AS Price
			,B.LineTotal AS LineTotal
			,B.TaxCode AS VatCode
			,B.WhsCode AS WarehouseCode
			,A.U_DeliCon AS DeliveryCondition
			,CASE WHEN E.ManBtchNum='Y' THEN
				'B'
			 WHEN E.ManSerNum='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS ManageItem
			,A.DocStatus AS [Status]
			,E.CodeBars AS BarCode
		FROM ORDR AS A
		LEFT JOIN RDR1 AS B ON A.DocEntry=B.DocEntry
		LEFT JOIN OCPR AS C ON A.CardCode=C.CardCode AND A.CntctCode=C.CntctCode
		--LEFT JOIN OUBR AS D ON D.Code=A.Branch
		LEFT JOIN OITM AS E ON E.ItemCode=B.ItemCode
		WHERE A.CardCode=CASE WHEN @par1='' THEN A.CardCode ELSE @par1 END AND A.DocNum=@par2 AND B.LineStatus= 'O';  
	END
	--End Sale Order --
	--Start Delivery --
	ELSE IF @TYPE='GET_Delivery'
	BEGIN
		SELECT 
				 DocNum AS DocNum
				 ,DocEntry AS DocEntry
				,CardCode AS VendorCode
				,CardName AS VendorName
				,DocTotal AS DocTotal 
				,NumAtCard AS VendorRefNo
				,Format(DocDate,'dd-MMM-yyyy') AS PostingDate
				FROM ODLN WHERE Series=case 
				when @par1='' Then Series Else @par1 End 
				AND DocStatus='O'
				--AND DocNum = Case when @par2='' Then DocNum Else Cast(@par2 AS INT) End
				AND DocNum LIKE '%' + @par2 + '%'
				AND U_WebID<>'' And ISNULL(U_docDraft,'')<>'oDraft' 
				ORDER BY DocNum DESC
				OFFSET cast(@par3 as int) ROWS
				FETCH NEXT cast(@par4 as int) ROWS ONLY;
			END
	ELSE IF @TYPE='GET_Reutrn_Count'
	BEGIN
		SELECT 
				COUNT(DocNum) AS "Count" FROM ODLN WHERE Series=case 
				when @par1='' Then Series Else @par1 End 
				AND DocStatus='O'
				--AND DocNum = Case when @par2='' Then DocNum Else Cast(@par2 AS INT) End
				AND DocNum LIKE '%' + @par2 + '%'
				AND U_WebID<>'' And ISNULL(U_docDraft,'')<>'oDraft' 
	END
	ELSE IF @TYPE='GET_Delivery_Detail_By_DocNum'
	BEGIN
		SELECT 
			 A.CardCode AS VendorCode
			,A.CardName AS VendorName
			,C.Name AS ContactPerson
			,A.CntctCode AS ContactPersonId
			,A.NumAtCard AS VendorNo
			,A.DocNum AS Remarks
			,A.BPLName AS BranchName
			,A.BPLId AS BranchId
			,A.DocEntry AS BaseDocEntry
			,B.LineNum AS BaseLineNumber
			,B.ItemCode AS ItemCode
			,B.Dscription AS ItemName
			,B.OpenQty-ISNULL(U_oDraftQty,0) AS Qty
			,B.Price AS Price
			,B.LineTotal AS LineTotal
			,B.TaxCode AS VatCode
			,B.WhsCode AS WarehouseCode
			,B.CodeBars AS BarCode
			,A.U_DeliCon AS DeliveryCondition
			,CASE WHEN E.ManBtchNum='Y' THEN
				'B'
			 WHEN E.ManSerNum='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS ManageItem
		FROM ODLN AS A
		LEFT JOIN DLN1 AS B ON A.DocEntry=B.DocEntry
		LEFT JOIN OCPR AS C ON A.CardCode=C.CardCode AND A.CntctCode=C.CntctCode
		LEFT JOIN OUBR AS D ON D.Code=A.Branch
		LEFT JOIN OITM AS E ON E.ItemCode=B.ItemCode
		WHERE  A.DocNum=@par2 AND B.LineStatus= 'O' And ISNULL(U_oDraftStatus,'')<>'C';
	END
	ELSE IF @TYPE='GetBatchSerialDelivery'
	BEGIN
		--nt docNum, string itemCode, string itemType
		IF @par3='S'
		BEGIN
			--SELECT 
			--	 B.DistNumber AS SerialBatch
			--	,B.LotNumber AS LotNo
			--	,B.ExpDate AS ExpDate
			--	,1 As Qty
			--	,B.SysNumber AS SysNumber
			--	,B.ItemCode AS ItemCode
			--FROM SRI1 AS A
			--LEFT JOIN OSRN AS B ON A.ItemCode=B.ItemCode AND B.SysNumber=A.SysSerial
			--WHERE A.BaseNum=@par1 AND A.ItemCode=@par2 AND A.BaseType=15
			Declare @DocEntry As INT

			SET @DocEntry=(Select DocEntry From ODLN Where DocNum=@par1)
			SELECT ItemCode,U_OutSerial INTO #TMPDLN FROM DLN1 WHERE DocEntry=@DocEntry

			SELECT 
				 B.DistNumber AS SerialBatch
				,B.LotNumber AS LotNo
				,B.ExpDate AS ExpDate
				,1 As Qty
				,B.SysNumber AS SysNumber
				,B.ItemCode AS ItemCode
				
			FROM SRI1 AS A
			LEFT JOIN OSRN AS B ON A.ItemCode=B.ItemCode AND B.SysNumber=A.SysSerial
			LEFT JOIN OSRI AS C ON C.ItemCode=A.ItemCode And C.SysSerial=A.SysSerial
			WHERE A.BaseNum=@par1 AND A.ItemCode=@par2 AND A.BaseType=15 And C.Status<>0 
			AND B.DistNumber NOT IN (SELECT B.DistNumber
			FROM SRI1 A
				LEFT JOIN OSRN B ON A.ItemCode=B.ItemCode And A.SysSerial=B.SysNumber
				LEFT JOIN OSRI AS C ON C.ItemCode=A.ItemCode And C.SysSerial=A.SysSerial
				WHERE  A.BaseType=16 And  C.Status<>0 And  A.ItemCode=@par2 ANd A.BsDocEntry=(Select DocEntry From ODLN WHERE DocNum=@par1)
			)
			--AND B.SysNumber NOT IN(SELECT Name FROM dbo.splitstring((ISNULL((SELECT U_OutSerial FROM #TMPDLN Z Where Z.ItemCode=@par2),'')),','))

			DROP TABLE #TMPDLN
		END
		ELSE IF @par3='B'
		BEGIN
			Declare @DocEntry1 As INT

			SET @DocEntry1=(Select DocEntry From ODLN Where DocNum=@par1)
			SELECT ItemCode,U_OutSerial INTO #TMPDLN1 FROM DLN1 WHERE DocEntry=@DocEntry1

			SELECT * FROM (
				SELECT 
					 A.BatchNum AS SerialBatch
					,B.LotNumber AS LotNo
					,B.ExpDate AS ExpDate
					,A.Quantity -ISNULL((SELECT SUM(Z.Quantity) FROM IBT1 Z WHERE BaseType=16 ANd A.BatchNum=Z.BatchNum And Z.ItemCode=A.ItemCode And A.BaseEntry=Z.BsDocEntry And A.WhsCode=Z.WhsCode),0) AS Qty
					--,A.Quantity AS Qty
					,B.SysNumber AS SysNumber
					,B.ItemCode AS ItemCode
				FROM IBT1 AS A 
				LEFT JOIN OBTN AS B ON A.ItemCode=B.ItemCode AND A.BatchNum=B.DistNumber
				WHERE A.BaseNum=@par1 AND A.ItemCode=@par2 AND A.BaseType=15
			)A wHERE Qty<>0 
			--And SysNumber NOT IN(SELECT Name FROM dbo.splitstring((ISNULL((SELECT U_OutSerial FROM #TMPDLN1 Z Where Z.ItemCode=@par2),'')),','))
			DROP TABLE #TMPDLN1
		END
		
	END
	ELSE IF @TYPE='BatchDetialDelivery'
	BEGIN
	--int docNum, string serialBatch
			SELECT 
				 B.DistNumber AS SerialBatch
				,B.LotNumber AS LotNo
				,B.ExpDate AS ExpDate
				,B.Quantity AS Qty
			FROM SRI1 AS A
			LEFT JOIN OSRN AS B ON A.ItemCode=B.ItemCode AND B.SysNumber=A.SysSerial
			WHERE A.BaseNum=@par1 AND B.DistNumber=@par2 AND A.BaseType=15
			UNION ALL
			SELECT 
				 A.BatchNum
				,B.LotNumber
				,B.ExpDate
				,A.Quantity
			FROM IBT1 AS A 
			LEFT JOIN OBTN AS B ON A.ItemCode=B.ItemCode AND A.BatchNum=B.DistNumber
			WHERE A.BaseNum=@par1 AND B.DistNumber=@par2  AND A.BaseType=15
	END
	--End Delivery --
	--Start Batch --
	ELSE IF @TYPE='BatchOrSerial'
	BEGIN
		IF @par1='S'
		BEGIN
			--SELECT 
			--	A.ItemCode AS ItemCode,
			--	A.DistNumber AS SerialOrBatch,
			--	A.Quantity AS Qty,
			--	A.SysNumber AS SysNumber ,
			--	A.ExpDate AS ExpDate,
			--	A.LotNumber AS LotNumber ,
			--	A.InDate AS AdmissionDate,
			--	ISNULL(CONVERT(nvarchar(MAX),A.ExpDate,111),'') AS ExpDate,
			--	ISNULL(A.LotNumber,'') AS LotNumber
			--	FROM OSRN AS A
			--	WHERE A.ItemCode=@par2 
			--	AND A.SysNumber NOT IN (SELECT SysSerial FROM SRI1 WHERE ItemCode=@par2 AND Direction=1)
			--	Order BY DistNumber;
			SELECT T0.ItemCode, T5.DistNumber As SerialOrBatch, Cast(T3.onHandQty As INT) As Qty
			,T5.SysNumber
			,Cast(Format(T5.InDate,'dd-MMM-yyyy') As Nvarchar(max)) As 'AdmissionDate',
			ISNULL(CONVERT(nvarchar(MAX),T4.ExpDate,111),'') AS ExpDate,
			ISNULL(T4.LotNumber,'') AS LotNumber
			-- T4.DistNumber, T4.MnfSerial,
			--T4.LotNumber, T5.MnfSerial, T5.LotNumber, T5.AbsEntry,
		    --T4.AbsEntry, T5.AbsEntry, T1.WhsCode,T0.BinAbs
		FROM
			OIBQ T0
			inner join OBIN T1 on T0.BinAbs = T1.AbsEntry and T0.onHandQty <> 0
			left outer join OBBQ T2 on T0.BinAbs = T2.BinAbs and T0.ItemCode = T2.ItemCode and T2.onHandQty <> 0
			left outer join OSBQ T3 on T0.BinAbs = T3.BinAbs and T0.ItemCode = T3.ItemCode and T3.onHandQty <> 0
			left outer join OBTN T4 on T2.SnBMDAbs = T4.AbsEntry and T2.ItemCode = T4.ItemCode
			left outer join OSRN T5 on T3.SnBMDAbs = T5.AbsEntry and T3.ItemCode = T5.ItemCode
		WHERE
			T1.AbsEntry >= 0 --and T1.WhsCode >= @WhsCode and T1.WhsCode <= @WhsCode 
			and (T3.AbsEntry is not null)
			and T0.ItemCode in((select U0.ItemCode from OITM U0 inner join OITB U1 on U0.ItmsGrpCod = U1.ItmsGrpCod
			WHERE U0.ItemCode is not null 
			))  And T1.WhsCode in(Select A.Name From dbo.splitstring(@par3,',') A) 
			And T0.ItemCode IN(Select A0.Name From dbo.splitstring(@par2,',') A0)
		END
		ELSE IF @par1='B'
		BEGIN
			SELECT 
				A.ItemCode AS ItemCode,
				A.DistNumber AS SerialOrBatch,
				B.Quantity AS Qty,
				A.SysNumber AS SysNumber,
				Format(A.ExpDate,'dd-MMM-yyyy') AS ExpDate,
				A.LotNumber AS LotNumber ,
				A.InDate AS AdmissionDate,
				ISNULL(CONVERT(nvarchar(MAX),A.ExpDate,111),'') AS ExpDate,
				ISNULL(A.LotNumber,'') AS LotNumber
			FROM OBTN AS A
			LEFT JOIN OBTQ AS B ON A.ItemCode=B.ItemCode AND A.SysNumber=B.SysNumber
			WHERE A.ItemCode=@par2 AND B.Quantity!=0 And B.WhsCode=@par3
		END
	END
	--End Batch --
	ELSE IF @TYPE='GET_Inventory_StockCount'
	BEGIN
		SELECT 
			 DocNum AS 'DocNum'
			,A.CountDate AS 'CounterDate'
			,CASE WHEN A.CountType='1' THEN 'Single Counter' WHEN A.CountType='2' THEN 'Multiple Counters' END AS 'CounterType'
			,CASE WHEN LEN(A.[Time])=3 THEN LEFT(A.[Time],1)+':'+RIGHT(A.[Time],2) ELSE LEFT(A.[Time],2)+':'+RIGHT(A.[Time],2) END AS 'CountTime' 
		FROM OINC AS A LEFT JOIN INC8 AS B ON A.DocEntry=B.DocEntry WHERE Series=@par1 AND B.CounterId=@par3 AND DocNum LIKE '%'+@par2+'%'
		--ORDER BY DocNum DESC
		
	END
	
	ELSE IF @TYPE='GET_Inventory_StockCount_Detail_ByDocNum'
	BEGIN
		SELECT 
			 A.CountDate AS CountDate
			,A.Series AS Series
			,A.DocNum AS DocNum
			,CASE WHEN A.CountType='1' THEN 'Single Counter' ELSE 'Multiple Counters' END AS CountType
			,A.IndvCount AS IndvCount
			,A.TeamCount AS TeamCount
			--,C.CounterNum AS CounterNum
			--,C.CounteName AS CounterName
			--,ISNULL(D.CounterNum,0) AS TeamCounterNum
			--,ISNULL(D.CounteName,'') AS TeamCounterName
			,A.[Status] AS 'Status'
			,A.Ref2 AS Ref
			,A.Remarks AS Remarks
			,B.ItemCode AS ItemCode
			,B.ItemDesc AS ItemName
			,B.InWhsQty AS Qty
			,B.WhsCode AS WarehouseCode
			,F.TotalQty AS QtyCounted
			,B.Counted AS Counted
			,B.LineNum AS LineNum
			,CASE WHEN E.ManBtchNum='Y' THEN
				'B'
			 WHEN E.ManSerNum='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS ManageItem
			,A.DocEntry AS DocEntry
		FROM OINC AS A
		LEFT JOIN INC1 AS B ON A.DocEntry=B.DocEntry
		LEFT JOIN OITM AS E ON E.ItemCode=B.ItemCode
		LEFT JOIN (
			SELECT INC9.DocEntry,INC8.CounterId,TotalQty FROM INC9 LEFT JOIN INC8 ON INC8.CounterNum=INC9.CounterNum AND INC8.DocEntry=INC9.DocEntry
		)As F ON A.DocEntry=F.DocEntry AND F.CounterId=@par2
		WHERE  A.DocNum=@par1
	END
	ELSE IF @TYPE='Number_of_Individual_Counter'
	BEGIN
		SELECT CounterId AS CounterNum,CounteName AS CounterName FROM INC8 WHERE DocEntry=@par1
	END
	ELSE IF @TYPE='Number_of_Multiple_Counter'
	BEGIN
		SELECT ISNULL(CounterNum,0) AS TeamCounterNum,ISNULL(CounteName,'') AS TeamCounterName FROM INC4  WHERE DocEntry=@par1
	END
	ELSE IF @TYPE='SERIES'
	BEGIN
		IF @par1='59'
		BEGIN
			SELECT Series,SeriesName,B.NextNumber As DocNum--,(SELECT ISNULL(MAX(DocNum)+1,B.InitialNum) FROM OIGN WHERE Series=B.Series)AS DocNum 
			FROM OFPR AS A 
			LEFT JOIN NNM1 AS B ON A.Indicator=B.Indicator
			WHERE B.Indicator=YEAR(GETDATE()) AND B.ObjectCode=59 AND SubNum=MONTH(GETDATE()) And B.Indicator<>'Default'
		END
		ELSE IF @par1='60'
		BEGIN
			SELECT 
				 Series
				,SeriesName
				,(SELECT ISNULL(MAX(DocNum)+1,B.InitialNum) FROM OIGE WHERE Series=B.Series)AS DocNum 
			FROM OFPR AS A 
			LEFT JOIN NNM1 AS B ON A.Indicator=B.Indicator
			WHERE B.Indicator=YEAR(GETDATE()) AND B.ObjectCode=60 AND SubNum=MONTH(GETDATE()) And B.Indicator<>'Default'
		END
		ELSE IF @par1='22'
		BEGIN
			SELECT 
				 Series
				,SeriesName
				,(SELECT ISNULL(MAX(DocNum)+1,B.InitialNum) FROM OIGE WHERE Series=B.Series)AS DocNum 
			FROM OFPR AS A 
			LEFT JOIN NNM1 AS B ON A.Indicator=B.Indicator
			WHERE B.Indicator=YEAR(GETDATE()) AND B.ObjectCode=22 AND SubNum=MONTH(GETDATE()) And B.Indicator<>'Default'
		END
		ELSE IF @par1='20'
		BEGIN
			SELECT 
				 Series
				,SeriesName
				,(SELECT ISNULL(MAX(DocNum)+1,B.InitialNum) FROM OPDN WHERE Series=B.Series)AS DocNum 
			FROM OFPR AS A 
			LEFT JOIN NNM1 AS B ON A.Indicator=B.Indicator
			WHERE B.Indicator=YEAR(GETDATE()) AND B.ObjectCode=20 AND SubNum=MONTH(GETDATE()) And B.Indicator<>'Default'
		END
		ELSE IF @par1='21'
		BEGIN
			SELECT 
				 Series
				,SeriesName
				,(SELECT ISNULL(MAX(DocNum)+1,B.InitialNum) FROM OPDN WHERE Series=B.Series)AS DocNum 
			FROM OFPR AS A 
			LEFT JOIN NNM1 AS B ON A.Indicator=B.Indicator
			WHERE B.Indicator=YEAR(GETDATE()) AND B.ObjectCode=21 AND SubNum=MONTH(GETDATE()) And B.Indicator<>'Default'
		END
		ELSE IF @par1='17'
		BEGIN
			SELECT 
				 Series
				,SeriesName
				,B.NextNumber As DocNum
			FROM OFPR AS A 
			LEFT JOIN NNM1 AS B ON A.Indicator=B.Indicator
			WHERE B.Indicator=YEAR(GETDATE()) AND 
				B.ObjectCode=17 AND SubNum=MONTH(GETDATE())
				And B.Indicator<>'Default'
		END
		ELSE IF @par1='15'
		BEGIN
			SELECT 
				 Series
				,SeriesName
				,B.NextNumber As DocNum
			--	,(SELECT ISNULL(MAX(DocNum)+1,B.InitialNum) FROM ORDR WHERE Series=B.Series)AS DocNum 
			FROM OFPR AS A 
			LEFT JOIN NNM1 AS B ON A.Indicator=B.Indicator
			WHERE B.Indicator=YEAR(GETDATE()) AND 
			B.ObjectCode=15 AND SubNum=MONTH(GETDATE())
			And B.Indicator<>'Default'
		END
		ELSE IF @par1='16'
		BEGIN
			SELECT 
				 Series
				,SeriesName
				,B.NextNumber As DocNum
				,(SELECT ISNULL(MAX(DocNum)+1,B.InitialNum) FROM ORDR WHERE Series=B.Series)AS DocNum 
			FROM OFPR AS A 
			LEFT JOIN NNM1 AS B ON A.Indicator=B.Indicator
			WHERE B.Indicator=YEAR(GETDATE()) AND B.ObjectCode=16 AND SubNum=MONTH(GETDATE()) And B.Indicator<>'Default'
		END
		ELSE IF @par1='1250000001'--Inventory Transfer Request
		BEGIN
			SELECT B.Series,SeriesName,B.BPLId As 'Branch',(SELECT ISNULL(MAX(DocNum)+1,B.InitialNum) 
			FROM OWTQ WHERE Series=B.Series)AS DocNum FROM OFPR AS A 
				LEFT JOIN NNM1 AS B ON A.Indicator=B.Indicator
			WHERE B.Indicator=YEAR(GETDATE()) AND B.ObjectCode=1250000001 AND A.SubNum=MONTH(GETDATE()) And B.Indicator<>'Default'
		END
		ELSE IF @par1='67'--Inventory Transfer Request
		BEGIN
			SELECT B.Series,SeriesName,B.BPLId As 'Branch',(SELECT ISNULL(MAX(DocNum)+1,B.InitialNum) 
			FROM OWTR WHERE Series=B.Series)AS DocNum FROM OFPR AS A 
				LEFT JOIN NNM1 AS B ON A.Indicator=B.Indicator
			WHERE B.Indicator=YEAR(GETDATE()) AND B.ObjectCode=67 AND A.SubNum=MONTH(GETDATE()) And B.Indicator<>'Default'
		END
		ELSE IF @par1='1470000065'
		BEGIN
			SELECT B.Series,SeriesName,B.BPLId As 'Branch',C.BPLName,(SELECT ISNULL(MAX(DocNum)+1,B.InitialNum) As DocNum	
			FROM OWTR WHERE Series=B.Series)AS DocNum FROM OFPR AS A 
				LEFT JOIN NNM1 AS B ON A.Indicator=B.Indicator
				LEFT JOIN OBPL AS C ON C.BPLId=B.BPLId
			WHERE B.Indicator=YEAR(GETDATE()) AND B.ObjectCode=1470000065 AND A.SubNum=MONTH(GETDATE()) And B.Indicator<>'Default'
		END
		ELSE IF @par1='202'
		BEGIN
			SELECT 
				 Series
				,SeriesName
				--,(SELECT ISNULL(MAX(DocNum)+1,B.InitialNum) FROM OIGE WHERE Series=B.Series)AS DocNum 
				,NextNumber As DocNum
			FROM OFPR AS A 
			LEFT JOIN NNM1 AS B ON A.Indicator=B.Indicator
			WHERE B.Indicator=YEAR(GETDATE()) AND B.ObjectCode=202 AND SubNum=MONTH(GETDATE()) And B.Indicator<>'Default'-- AND B.BPLId=7
		END
		ELSE IF @par1='13'
		BEGIN
			SELECT 
				 Series
				,SeriesName
				,A.NextNumber As DocNum
			FROM NNM1  A 
			WHERE A.Indicator=YEAR(GETDATE()) AND A.ObjectCode=13 And A.Indicator<>'Default' 
		END
	
		ELSE IF @par1='14'
		BEGIN
			SELECT 
				 Series
				,SeriesName
				,A.NextNumber As DocNum
			FROM NNM1  A 
			WHERE A.Indicator=YEAR(GETDATE()) AND A.ObjectCode=14 And A.Indicator<>'Default' 
		END
		ELSE IF @par1='67'
		BEGIN
			SELECT 
				 Series
				,SeriesName
				,A.NextNumber As DocNum
			FROM NNM1  A 
			WHERE A.Indicator=YEAR(GETDATE()) AND A.ObjectCode=67 And A.Indicator<>'Default' 
		END
	END
	ELSE IF @TYPE='GetItemCodeByBarCode'
	BEGIN
		SELECT   ItemCode AS ItemCode
				,ItemName AS ItemName
				,(SELECT Price FROM ITM1 WHERE PriceList=1 AND ItemCode=OITM.ItemCode) AS PriceUnit
		FROM OITM WHERE CodeBars=@par1
	END
	ELSE IF @TYPE='GetVendor'
	BEGIN
		SELECT   CardCode AS VendorCode
				,CardName As VendorName
				,Phone1 As PhoneNumber
				,CntctPrsn as ContactID
		FROM OCRD WHERE CardType='S'
	END
	ELSE IF @TYPE='GetBranch'
	BEGIN
		SELECT  BPLid AS BranchID
			   ,BPLName AS BranchName
		FROM OBPL WHERE [Disabled]!='Y'
	END
	ELSE IF @TYPE='GetContactPersonByCardCode'
	BEGIN
		SELECT  
			CntctCode AS ContactID,
			Name As ContactName
		FROM OCPR WHERE CardCode=@par1
	END
	ELSE IF @TYPE='GetItem'
	BEGIN
		SELECT   ItemCode AS ItemCode
				,ItemName AS ItemName
				,(SELECT Price FROM ITM1 WHERE PriceList=1 AND ItemCode=OITM.ItemCode) AS PriceUnit
		FROM OITM 
	END
	ELSE IF @TYPE='GennerateBatchOrSerial'
	BEGIN 
		SELECT @par1+@par2+CAST(YEAR(GETDATE()) AS nvarchar(MAX))+CAST(MONTH(GETDATE()) AS nvarchar(MAX))+CAST(DAY(GETDATE()) AS nvarchar(MAX))+CAST(SYSDATETIME() AS nvarchar(MAX)) AS BatchOrSerial;
	END
	ELSE IF @TYPE='WhsCode'
	BEGIN
		--SELECT WhsCode AS Code,WhsName AS Name FROM OWHS WHERE Inactive='N';
		SELECT 
		WhsCode AS Code
		,WhsName AS Name 
		,A0.BPLName AS BranchName
		FROM OWHS AS A
		LEFT JOIN OBPL AS A0 ON A0.BPLId=A.BPLid
		WHERE Inactive='N';
	END
	ELSE IF @TYPE='GetTransferRequest'
	BEGIN
		SELECT 
				 DocNum AS DocNum
				,CardCode AS CardCode
				,CardName AS CardName
				,DocTotal AS DocTotal FROM OPOR WHERE Series=CASE WHEN @par1=0 THEN Series ELSE @par1 END AND DocNum LIKE '%'+@par2+'%';
	END
		ELSE IF @TYPE='GET_Inventory_Transfer_Request'
	BEGIN
		SELECT DocNum,Cast(FORMAT(DocDate,'yyyy-MM-dd') As Nvarchar(max)) As 'DocDate',BPLName As Branch,Filler As 'FromWarehouse',ToWhsCode As 'ToWarehouse' 
		FROM OWTQ WHERE DocStatus='O' And Series=@par1 AND DocNum LIKE '%'+@par2+'%';
	
	END
	ELSE IF @TYPE='GET_Inventory_Request_DetailByDocNum'
	BEGIN	
		SELECT --
			Cast(FORMAT(A.DocDate,'yyyy-MM-dd') As Nvarchar(max)) As DocDate,Cast(FORMAT(ShipDate,'yyyy-MM-dd') As Nvarchar(max)) As ShipDate			
			,A.CardCode,A.CardName,A.NumAtCard,A.[Address],A.JrnlMemo As Remark
			,A.BPLId,A.BPLName,A.Filler As HWhsFrom,A.ToWhsCode As HWhsTo
			,A.DocEntry,B.LineNum,B.ItemCode,Dscription,Cast(B.OpenQty As INT) As Qty
			,B.Price
			,B.LineTotal
			,UomCode,B.FromWhsCod,B.WhsCode,ISNULL(C.ManBtchNum,'')+ISNULL(C.ManSerNum,'') As ManItem
			,A.BPLName As Branch
		FROM OWTQ A
			LEFT JOIN WTQ1 B ON A.DocEntry=B.DocEntry
			LEFT JOIN OITM C ON B.ItemCode=C.ItemCode
		WHERE A.DocStatus='O' And A.DocNum IN(@par1) And B.LineStatus='O' --LIKE '%'+@par2+'%';
	END
	ELSE IF @TYPE='GET_Production_Order'
	BEGIN
	SELECT * FROM(
		SELECT 
			 A.DocEntry AS DocEntry
			,A.DocNum AS DocNum
			,Cast(Format(A.DueDate,'dd-MMM-yyyy') As Nvarchar(max)) AS DueDate
			,A.ItemCode AS ProductNo
			,C.ItemName AS ProductName
			,A.PlannedQty-A.CmpltQty AS Qty
			,D.Price AS Price
			,CASE WHEN C.ManBtchNum='Y' THEN
				'B'
			 WHEN C.ManSerNum='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS ItemType
			,C.CodeBars
			,Cast(Format(A.StartDate,'dd-MMM-yyyy') As Nvarchar(max)) AS StartDate
			,E.BPLid AS BranchCode
			,F.BPLName AS BranchName
			,A.Warehouse
			,A.Uom
		FROM OWOR AS A
		LEFT JOIN OITM AS C ON C.ItemCode=A.ItemCode
		LEFT JOIN ITM1 AS D ON A.ItemCode=D.ItemCode AND PriceList=1
		LEFT JOIN OWHS AS E ON E.WhsCode=A.Warehouse
		LEFT JOIN OBPL AS F ON E.BPLid=F.BPLId
		WHERE A.Series=CASE WHEN @par1='-1' THEN A.Series ELSE @par1 END 
			  AND A.Status='R'
			  --AND MONTH(A.StartDate)=CASE WHEN @par1='-1' THEN MONTH(GETDATE()) ELSE MONTH(A.StartDate) END
			  AND A.DocEntry NOT IN (SELECT BaseEntry FROM IGN1 WHERE BaseType=202)
			  AND DocNum LIKE '%'+@par2+'%'
	)A WHERE A.Qty<>0
		ORDER BY DocNum DESC
		OFFSET cast(@par3 as int) ROWS
		FETCH NEXT cast(@par4 as int) ROWS ONLY;
	END
	ELSE IF @TYPE='GET_Production_Order_Count'
	BEGIN
	SELECT COUNT(A.DocEntry) AS 'COUNT' FROM(
		SELECT 
			 A.DocEntry AS DocEntry
			,A.PlannedQty-A.CmpltQty AS Qty
		FROM OWOR AS A
		LEFT JOIN OITM AS C ON C.ItemCode=A.ItemCode
		LEFT JOIN ITM1 AS D ON A.ItemCode=D.ItemCode AND PriceList=1
		LEFT JOIN OWHS AS E ON E.WhsCode=A.Warehouse
		LEFT JOIN OBPL AS F ON E.BPLid=F.BPLId
		WHERE A.Series=CASE WHEN @par1='-1' THEN A.Series ELSE @par1 END 
			  AND A.Status='R'
			  --AND MONTH(A.StartDate)=CASE WHEN @par1='-1' THEN MONTH(GETDATE()) ELSE MONTH(A.StartDate) END
			  AND A.DocEntry NOT IN (SELECT BaseEntry FROM IGN1 WHERE BaseType=202)
			  AND DocNum LIKE '%'+@par2+'%'
	)A WHERE A.Qty<>0
	END
	ELSE IF @TYPE='GET_Production_Order_Web'
	BEGIN
	SELECT * FROM
	(
		SELECT 
			 A.DocEntry AS DocEntry
			,A.DocNum AS DocNum
			,Cast(Format(A.DueDate,'dd-MMM-yyyy') As Nvarchar(max)) AS DueDate
			,A.ItemCode AS ProductNo
			,C.ItemName AS ProductName
			,A.PlannedQty-A.CmpltQty AS Qty
			,D.Price AS Price
			,A.Status ItemType
			,C.CodeBars
			,Cast(Format(A.StartDate,'dd-MMM-yyyy') As Nvarchar(max)) AS StartDate
			,E.BPLid AS BranchCode
			,F.BPLName AS BranchName
			,A.Warehouse
			,A.Uom
		FROM OWOR AS A
		LEFT JOIN OITM AS C ON C.ItemCode=A.ItemCode
		LEFT JOIN ITM1 AS D ON A.ItemCode=D.ItemCode AND PriceList=1
		LEFT JOIN OWHS AS E ON E.WhsCode=A.Warehouse
		LEFT JOIN OBPL AS F ON E.BPLid=F.BPLId
		WHERE A.Series=CASE WHEN @par1='-1' THEN A.Series ELSE @par1 END 
			AND A.DocEntry NOT IN (SELECT BaseEntry FROM IGN1 WHERE BaseType=202)
			AND A.Status='P'
			AND A.U_WebID IS NOT NULL
			AND DocNum LIKE '%'+@par2+'%'
	) A WHERE A.Qty<>0
	END
	ELSE IF @TYPE='SerialStatus'
	BEGIN
	SELECT * FROM(
			SELECT T0.ItemCode, T5.DistNumber As [SerialBatch], Cast(T3.onHandQty As INT) As Qty
			,T1.WhsCode,T1.BinCode,Format(T5.ExpDate,'dd-MMM-yyyy') As 'ExpDate',T5.SysNumber
			,T4.LotNumber,Cast(Format(T5.MnfDate,'dd-MMM-yyyy') As Nvarchar(max)) As 'MnfDate' ,Cast(Format(T5.InDate,'dd-MMM-yyyy') As Nvarchar(max)) As 'AdmissionDate'
			,T5.MnfSerial As MfrNo
			,'Available' As SerialStatus
			-- T4.DistNumber, T4.MnfSerial,
			--T4.LotNumber, T5.MnfSerial, T5.LotNumber, T5.AbsEntry,
		    --T4.AbsEntry, T5.AbsEntry, T1.WhsCode,T0.BinAbs
		FROM
			OIBQ T0
			inner join OBIN T1 on T0.BinAbs = T1.AbsEntry and T0.onHandQty <> 0
			left outer join OBBQ T2 on T0.BinAbs = T2.BinAbs and T0.ItemCode = T2.ItemCode and T2.onHandQty <> 0
			left outer join OSBQ T3 on T0.BinAbs = T3.BinAbs and T0.ItemCode = T3.ItemCode and T3.onHandQty <> 0
			left outer join OBTN T4 on T2.SnBMDAbs = T4.AbsEntry and T2.ItemCode = T4.ItemCode
			left outer join OSRN T5 on T3.SnBMDAbs = T5.AbsEntry and T3.ItemCode = T5.ItemCode
		WHERE
			T1.AbsEntry >= 0 --and T1.WhsCode >= @WhsCode and T1.WhsCode <= @WhsCode 
			and (T3.AbsEntry is not null)
			and T0.ItemCode in((select U0.ItemCode from OITM U0 inner join OITB U1 on U0.ItmsGrpCod = U1.ItmsGrpCod
			WHERE U0.ItemCode is not null 
			))  And T0.ItemCode IN(Select A0.Name From dbo.splitstring(@par2,',') A0)
		UNION ALL
		SELECT A.ItemCode,B.DistNumber,A.Quantity As Qty,A.WhsCode,'' As BinCode
		,B.ExpDate,A.SysNumber,B.LotNumber,B.MnfDate,B.InDate As AdmissionDate,B.MnfSerial
		,'UnAvailable' As SerialStatus
		FROM OSRQ A 
			LEFT JOIN OSRN B ON A.ItemCode=B.ItemCode And A.SysNumber=B.SysNumber And A.MdAbsEntry=B.AbsEntry
			LEFT JOIN OWHS C ON C.WhsCode=A.WhsCode 
		--	LEFT JOIN OBIN C ON B.AbsEntry=B.AbsEntry
		WHERE A.Quantity<>0 and A.ItemCode IN(Select A0.Name From dbo.splitstring(@par2,',') A0) And C.BinActivat='N' --'ACCBINSTOB55'
		)A WHERE WhsCode=@par1 AND ItemCode=@par2
	END
	ELSE IF @TYPE='Calllayout'
	BEGIN 
		--SELECT * FROM(
		--SELECT
		--	'1' AS 'Code'
		--	,'Testing2.rdl' AS 'FILENAME'
		--	,'PDF' AS EXPORTTYPE
		--	,'USP_CALLTRANSCATION_POSKOFI' AS STOREPROCEDURE
		--	,'[{"TypeofParameter":"USP_KOFI_BARCODE_PRINTING","DataSetName":"DataSet1"}]' AS PROPeRTIES
		--	WHERE 1=1
		--	)A
		--	WHERE A.Code=1
			
			SELECT Code,U_FileName AS 'FILENAME',U_ExportType As EXPORTTYPE,U_StoreProcedure As STOREPROCEDURE,U_Properties As PROPeRTIES 
			FROM [@TBKOFIWEBPRINTING]
			WHERE Code=@par1
		END
	ELSE IF @TYPE='Print_Barcode'
	BEGIN
		SELECT TOP 10 ItemCode,'100.38' As Price,'*'+CodeBars+'*' As CodeBars FROM OITM WHERE ISNULL(CodeBars,'')<>''
	END
	ELSE IF @TYPE='GETLayouts'
	BEGIN
		SELECT Code,Name,U_Addess As [Address]
		FROM [@TBKOFIWEBPRINTING]
		WHERE U_LayoutModule=@par1
		
	END
	ELSE IF @TYPE='USP_KOFI_BARCODE_PRINTING'
	BEGIN 
		SELECT ROW_NUMBER() OVER(ORDER BY DocNum) As #No,T0.DocEntry,T5.SeriesName,T0.CardCode,T0.CardName,T0.DocNum,T0.DocTotal,T0.DocDate,T0.DocDueDate,T0.NumAtCard,T0.Comments,T0.BPLName As Branch
				,T1.ItemCode,T1.Dscription,Cast(T1.Quantity As INT) AS Quantity,Cast(T1.Price As Numeric(9,2)) As Price,Cast(T1.LineTotal As Numeric(9,2)) As LineTotal,T1.BaseEntry,T1.BaseLine,T1.UomCode
				,T1.WhsCode,IIF(T4.ManBtchNum='Y','B',IIF(T4.ManSerNum='Y','S','N')) As ItemType,T4.CodeBars
				,Cast(T0.DocTotal As Numeric(9,2)) As DocTotal,U_docDraft As PONumber
				,T0.BPLName
			FROM ODRF T0

			INNER JOIN DRF1 T1 ON T0.DocEntry=T1.DocEntry

			LEFT JOIN DRF16 T2 ON T1.DocEntry=T2.AbsEntry AND T1.LineNum=T2.LineNum

			LEFT JOIN ODBN T3 ON T2.ObjId=T3.ObjType AND T2.ObjAbs=T3.AbsEntry

			LEFT JOIN OITM T4 ON T4.ItemCode=T1.ItemCode

			LEFT JOIN NNM1 T5 ON T5.Series=T0.Series And T5.ObjectCode=20

			WHERE T0.ObjType=20  And T0.DocEntry=@par1
	END
	ElSE IF @TYPE='Delivery_Print'
	BEGIN
		select  
		D0.CardCode
		,D0.Address AS ShipTO
		,D0.Address2 AS BillTo
		,D0.DocNum
		,D0.DocDate
		,D0.U_DeliCon AS DeliveryCOndition
		,D1.ItemCode
		,D1.Dscription AS Description
		,D1.Quantity As Qty
		From ODLN AS D0 
		LEFT JOIN DLN1 D1 ON D0.DocEntry=D1.DocEntry
		--select *From ODLN
		where D0.DocEntry=@par1
	END
	ElSE IF @TYPE='Return_Delivery_Print'
	BEGIN
		--select  top 1
		--D0.CardCode
		--,D0.Address AS ShipTO
		--,D0.Address2 AS BillTo
		--,D0.DocNum
		--,D0.DocDate
		--,D0.U_DeliCon AS DeliveryCOndition
		--,D1.ItemCode
		--,D1.Dscription AS Description
		--,D1.Quantity As Qty
		--From ODLN AS D0 
		--LEFT JOIN DLN1 D1 ON D0.DocEntry=D1.DocEntry
		----select *From ODLN
		----where D0.DocEntry=@par1
		SELECT ROW_NUMBER() OVER(ORDER BY DocNum) As #No,T0.DocEntry,T5.SeriesName,T0.CardCode,T0.CardName
		,T0.DocNum,T0.DocTotal,Format(T0.DocDate,'dd-MMM-yyyy') As DocDate,T0.DocDueDate,T0.NumAtCard,T0.Comments,T0.BPLName As Branch
			,T1.ItemCode,T1.Dscription As [Description]
			,Cast(T1.Quantity As INT) AS Qty,Cast(T1.Price As Numeric(9,2)) As Price
			,Cast(T1.LineTotal As Numeric(9,2)) As LineTotal,T1.DiscPrcnt
			,T1.BaseEntry,T1.BaseLine,T1.UomCode,T6.Phone1,T6.Phone2,T0.Address2 As [BillTo],T0.Address As [ShipTo]
			,T1.WhsCode,IIF(T4.ManBtchNum='Y','B',IIF(T4.ManSerNum='Y','S','N')) As ItemType,T4.CodeBars
			,Cast(T0.DocTotal As Numeric(9,2)) As DocTotal,U_docDraft As PONumber
			,T0.BPLName
			,C.ItmsGrpNam As Brand
			,T0.U_docDraft As BaseNum
			,'' As SerialBatch
			,T1.BaseLine As BaseLinNum
			,T0.U_DeliCon As DeliveryCondition
		FROM ODRF T0

		INNER JOIN DRF1 T1 ON T0.DocEntry=T1.DocEntry

		LEFT JOIN DRF16 T2 ON T1.DocEntry=T2.AbsEntry AND T1.LineNum=T2.LineNum

		LEFT JOIN ODBN T3 ON T2.ObjId=T3.ObjType AND T2.ObjAbs=T3.AbsEntry

		LEFT JOIN OITM T4 ON T4.ItemCode=T1.ItemCode

		LEFT JOIN NNM1 T5 ON T5.Series=T0.Series And T5.ObjectCode=16

		LEFT JOIN OCRD T6 ON T6.CardCode=T0.CardCode

		LEFT JOIN KPPT B On T1.ItemCode=B.ItemCode

		LEFT JOIN OITG C ON C.ItmsTypCod=B.ItmsTypCod

		WHERE T0.ObjType=16 and T1.DocEntry=@par1--  And T0.DocEntry=@par1
	END
	ElSE IF @TYPE='Credit_Memo_Print'
	BEGIN
		
		SELECT ROW_NUMBER() OVER(ORDER BY DocNum) As #No,T0.DocEntry,T5.SeriesName,T0.CardCode,T0.CardName,T0.DocNum,T0.DocTotal,T0.DocDate,T0.DocDueDate,T0.NumAtCard,T0.Comments,T0.BPLName As Branch
			,T1.ItemCode,T1.Dscription
			,Cast(T1.Quantity As INT) AS Quantity,Cast(T1.Price As Numeric(9,2)) As Price
			,Cast(T1.LineTotal As Numeric(9,2)) As LineTotal,T1.DiscPrcnt
			,T1.BaseEntry,T1.BaseLine,T1.UomCode,T6.Phone1,T6.Phone2,T0.Address2,T0.Address
			,T1.WhsCode,IIF(T4.ManBtchNum='Y','B',IIF(T4.ManSerNum='Y','S','N')) As ItemType,T4.CodeBars
			,Cast(T0.DocTotal As Numeric(9,2)) As DocTotal,U_docDraft As PONumber
			,T0.BPLName
				
		FROM ODRF T0

		INNER JOIN DRF1 T1 ON T0.DocEntry=T1.DocEntry

		LEFT JOIN DRF16 T2 ON T1.DocEntry=T2.AbsEntry AND T1.LineNum=T2.LineNum

		LEFT JOIN ODBN T3 ON T2.ObjId=T3.ObjType AND T2.ObjAbs=T3.AbsEntry

		LEFT JOIN OITM T4 ON T4.ItemCode=T1.ItemCode

		LEFT JOIN NNM1 T5 ON T5.Series=T0.Series And T5.ObjectCode=14

		LEFT JOIN OCRD T6 ON T6.CardCode=T0.CardCode

		WHERE T0.ObjType=14 and T1.DocEntry=@par1--  And T0.DocEntry=@par1


		
	END
	ELSE IF @TYPE='Goods_Return' 
	BEGIN 
	Select ROW_NUMBER() Over(Order by BaseLinNum) AS #No,C.ItmsGrpNam AS Brand,A.* From(
		SELECT	A.CardCode
				,A.CardName
				,A.DocDate,
				A.BaseNum,
				A.ItemCode AS ItemCode	
				,B.DistNumber AS SerialBatch
				,1 AS Qty
				,A.BaseLinNum

			FROM SRI1 AS A
			LEFT JOIN OSRN AS B ON A.ItemCode=B.ItemCode AND B.SysNumber=A.SysSerial
			LEFT JOIN RPD1 C ON C.DocEntry=A.BaseEntry And C.VisOrder=A.BaseLinNum
			LEFT JOIN ORPD D ON D.DocEntry=C.DocEntry
			WHERE A.BaseType=21 and d.DocEntry=19

			UNION All

			Select G0.CardCode,G0.CardName,G0.DocDate,G1.BaseDocNum As BaseNum, G1.ItemCode,'' AS SerialBacth, G1.Quantity As Qty, G1.VisOrder As BaseLinNum
			FROM ORPD AS  G0
			LEFT JOIN RPD1 G1 ON G1.DocEntry=G0.DocEntry
			LEFT JOIN OITM G2 ON G1.ItemCode=G2.ItemCode

			where G0.DocEntry=@par1 and ManSerNum<>'Y'
			) As A
			LEFT JOIN KPPT B On A.ItemCode=B.ItemCode
			LEFT JOIN OITG C ON C.ItmsTypCod=B.ItmsTypCod

			


	END
	ELSE IF @TYPE='Draft'
	BEGIN
		IF @par1='20'
		BEGIN
			SELECT T0.DocNum,T0.DocDate,T0.CardCode,T0.CardName,T0.DocTotal,T0.NumAtCard,T0.BPLName 
			FROM ODRF T0

			INNER JOIN DRF1 T1 ON T0.DocEntry=T1.DocEntry

			LEFT JOIN DRF16 T2 ON T1.DocEntry=T2.AbsEntry AND T1.LineNum=T2.LineNum

			LEFT JOIN ODBN T3 ON T2.ObjId=T3.ObjType AND T2.ObjAbs=T3.AbsEntry

			WHERE T0.ObjType=20 And T0.DocStatus='O' 
		END
		IF @par1='20Line'
		BEGIN
			SELECT T5.SeriesName,T0.CardCode,T0.CardName,T0.DocNum,T0.DocTotal,T0.DocDate,T0.DocDueDate,T0.NumAtCard,T0.Comments,T0.BPLName As Branch
				,T1.ItemCode,T1.Dscription,T1.Quantity,T1.Price,T1.LineTotal,T1.BaseEntry,T1.BaseLine,T1.UomCode
				,T1.WhsCode,IIF(T4.ManBtchNum='Y','B',IIF(T4.ManSerNum='Y','S','N')) As ItemType,T4.CodeBars
			FROM ODRF T0

			INNER JOIN DRF1 T1 ON T0.DocEntry=T1.DocEntry

			LEFT JOIN DRF16 T2 ON T1.DocEntry=T2.AbsEntry AND T1.LineNum=T2.LineNum

			LEFT JOIN ODBN T3 ON T2.ObjId=T3.ObjType AND T2.ObjAbs=T3.AbsEntry

			LEFT JOIN OITM T4 ON T4.ItemCode=T1.ItemCode

			LEFT JOIN NNM1 T5 ON T5.Series=T0.Series And T5.ObjectCode=20

			WHERE T0.ObjType=20 And T0.DocStatus='O'  And T0.DocNum=@par2
		END
	END
	ELSE IF @TYPE='ARDocumentHeader'
	BEGIN
	SELECT  DocNum,DocDate,CardCode,CardName,NumAtCard,BPLName,DocTotal,U_docDraft FROM (
		SELECT DISTINCT
		 A.DocNum AS DocNum
		,Format(A.DocDate,'dd/MMM/yyyy') As DocDate
		,A.CardCode
		,A.CardName
		,A.NumAtCard
		,A.BPLName
		,A.DocTotal
		,A.U_docDraft
		,A.docentry
		FROM OINV A 
			LEFT JOIN NNM1 B ON A.Series=B.Series And B.ObjectCode=13
			LEFT JOIN INV1 C ON A.DocEntry=C.DocEntry
			LEFT JOIN OITM D ON D.ItemCode=C.ItemCode
		--AND ISNULL(A.U_docDraft,'') <>'oDraft'	 
		WHERE A.Series=case when @par1='' Then A.Series  When @par1='All' Then A.Series Else @par1 End 
		--AND DocNum = Case when @par2='' Then DocNum Else Cast(@par2 AS INT) End
		AND DocNum LIKE '%' + @par2 + '%'
		AND C.LineStatus='O' AND A.DocStatus='O' AND A.DocType='I' AND ISNULL(A.U_docDraft,'')<>'oDraft' --And (D.ManBtchNum='Y' OR D.ManSerNum='Y')
		
		)A ORDER BY DocEntry DESC
		OFFSET cast(@par3 as int) ROWS
		FETCH NEXT cast(@par4 as int) ROWS ONLY;
	END
	ELSE IF @TYPE='ARDocumentHeaderCount'
	BEGIN

	SELECT COUNT(A.DocNum) AS "Count" FROM (
		SELECT DISTINCT DocNum AS DocNum
		FROM OINV A 
			LEFT JOIN NNM1 B ON A.Series=B.Series And B.ObjectCode=13
			LEFT JOIN INV1 C ON A.DocEntry=C.DocEntry
			LEFT JOIN OITM D ON D.ItemCode=C.ItemCode
		--AND ISNULL(A.U_docDraft,'') <>'oDraft'	 
		WHERE A.Series=case when @par1='' Then A.Series  When @par1='All' Then A.Series Else @par1 End 
		--AND DocNum = Case when @par2='' Then DocNum Else Cast(@par2 AS INT) End
		AND DocNum LIKE '%' + @par2 + '%'
		AND C.LineStatus='O' AND A.DocStatus='O' AND A.DocType='I' AND ISNULL(A.U_docDraft,'')<>'oDraft' --And (D.ManBtchNum='Y' OR D.ManSerNum='Y')
	) A 

	END
	ELSE IF @TYPE='ARDocumentLine'
	BEGIN
		SELECT B.DocNum,A.LineNum,B.DocDate,B.DocDueDate,B.CardCode,B.CardName,ISNULL(B.NumAtCard,'') As NumAtCard,C.SeriesName,B.BPLId,B.BPLName,B.DocTotal
			,A.ItemCode,A.Dscription
			--,A.OpenQty-ISNULL(A.U_oDraftQty	,0) As Quantity
			,A.OpenQty As Quantity
			,A.OpenQty,A.Price,A.LineTotal,A.UomCode,A.VatGroup,A.WhsCode
			,A.LineNum,A.DocEntry,D.CodeBars
			,IIF(D.ManBtchNum='Y','B',IIF(D.ManSerNum='Y','S','N')) As ManItem
			,ISNULL(B.Comments,'') As Comments
		FROM INV1 A 
			LEFT JOIN OINV B ON A.DocEntry=B.DocEntry
			LEFT JOIN NNM1 C ON C.Series=B.Series And C.ObjectCode=13
			LEFT JOIN OITM D ON D.ItemCode=A.ItemCode
		WHERE B.DocType='I' AND B.DocStatus='O' AND A.LineStatus='O' --And ISNULL(A.U_oDraftStatus,'')<>'C'
			And B.DocNum=@par1
	
	END
	ELSE IF @TYPE='GetARSerial'
	BEGIN
		Declare @U_Serial As Nvarchar(max)
		Declare @AREntry As INT

		SET @AREntry=(SELECT DocEntry FROM OINV WHERE DocNum=@par1)
		SET @U_Serial=(SELECT ISNULL(U_OutSerial,'') FROM INV1 WHERE DocEntry=@AREntry And ItemCode=@par2)
			 SELECT B.ItemCode,IIF(B.BaseType=15,C.Serial,D.Serial) As Serial
					,1 As Qty
				   ,IIF(B.BaseType=15,C.LotNumber,D.LotNumber) As LotNumber
				   ,IIF(B.BaseType=15,C.ExpDate,D.ExpDate) As ExpDate
				   ,IIF(B.BaseType=15,C.MnfSerial,D.MnfSerial) As MnfSerial
				   ,IIF(B.BaseType=15,C.MnfDate,D.MnfDate) As MnfDate
				   ,IIF(B.BaseType=15,C.SysSerial,D.SysNumber) As SysSerial
				   ,IIF(B.BaseType=15,C.InDate,D.InDate) As AdmissionDate
				    ,IIF(ISNULL(C.OnHandQty,0)>0,C.OnHandQty,ISNULL(D.OnHandQty,0)) As OnHandQty
				   INTO #TMPAR_Memo
			 FROM OINV A
				 LEFT JOIN INV1 B ON A.DocEntry=B.DocEntry
				 LEFT JOIN DLN1 T0 ON T0.DocEntry=B.BaseEntry And T0.LineNum=B.BaseLine
				 LEFT JOIN ODLN T1 ON T0.DocEntry=T1.DocEntry
				 LEFT JOIN OITM I ON I.ItemCode=B.ItemCode
				 LEFT JOIN (
					SELECT T0.BaseNum,T0.ItemCode,T0.BaseLinNum,T1.DistNumber As Serial,T1.LotNumber,Format(ExpDate,'dd-MMM-yyyy') AS ExpDate 
						,1 As Qty,T1.MnfSerial,T1.MnfDate,T0.SysSerial,Format(T1.CreateDate,'dd-MMM-yyyy') As InDate,T2.OnHandQty
						FROM SRI1 T0
						LEFT JOIN OSRN T1 ON T0.ItemCode=T1.ItemCode And T0.SysSerial=T1.SysNumber
						LEFT JOIN OSBQ T2 ON T2.SnBMDAbs=T1.AbsEntry And T2.ItemCode=T1.ItemCode 
						WHERE T0.BaseType=15
				 )C ON C.BaseNum=T1.DocNum And B.ItemCode=C.ItemCode
				 LEFT JOIN (
					SELECT T0.BaseNum,T0.ItemCode,T0.BaseLinNum,T1.DistNumber As Serial,T1.LotNumber,Format(ExpDate,'dd-MMM-yyyy') AS ExpDate 
						,1 As Qty,T1.MnfSerial,T1.MnfDate,T1.SysNumber,Format(T1.CreateDate,'dd-MMM-yyyy') As InDate,T2.OnHandQty
					FROM SRI1 T0
						LEFT JOIN OSRN T1 ON T0.ItemCode=T1.ItemCode And T0.SysSerial=T1.SysNumber
						LEFT JOIN OSBQ T2 ON T2.SnBMDAbs=T1.AbsEntry And T2.ItemCode=T1.ItemCode
						WHERE T0.BaseType=13
				 )D ON D.BaseNum=A.DocNum And B.ItemCode=D.ItemCode
			 WHERE A.CANCELED='N' And I.ManSerNum='Y'
			  And A.DocNum=@par1 And B.ItemCode=@par2

			SELECT distinct * FROM #TMPAR_Memo WHERE OnHandQty=0--SysSerial NOT IN(SELECT Name FROM dbo.splitstring(@U_Serial,','))
			
			DROP TABLE #TMPAR_Memo

	END
	ELSE IF @TYPE='GetARBatch'
	BEGIN
	Declare @NumberDO AS INT
	Set @NumberDO=(
		SELECT Top 1 T1.DocNum
		FROM OINV A
			LEFT JOIN INV1 B ON A.DocEntry=B.DocEntry
			LEFT JOIN DLN1 T0 ON T0.DocEntry=B.BaseEntry And T0.LineNum=B.BaseLine
			LEFT JOIN ODLN T1 ON T0.DocEntry=T1.DocEntry
			Where A.DocNum=@par1 AND A.DocStatus='O')

		select distinct * from (
			SELECT B.ItemCode,IIF(B.BaseType=15,C.Batch,D.Batch) As Batch
					,IIF(B.BaseType=15,C.Qty,D.Qty) As Qty
				    ,IIF(B.BaseType=15,C.LotNumber,D.LotNumber) As LotNumber
				   ,IIF(B.BaseType=15,C.ExpDate,D.ExpDate) As ExpDate
				   ,IIF(B.BaseType=15,C.MnfSerial,D.MnfSerial) As MnfSerial
				   ,IIF(B.BaseType=15,C.MnfDate,D.MnfDate) As MnfDate
				   ,IIF(B.BaseType=15,C.SysNumber,D.SysNumber) As SysNumber
				   ,IIF(B.BaseType=15,C.InDate,D.InDate) As AdmissionDate
				  -- ,A.DocNum
			 FROM OINV A
				 LEFT JOIN INV1 B ON A.DocEntry=B.DocEntry
				 LEFT JOIN DLN1 T0 ON T0.DocEntry=B.BaseEntry And T0.LineNum=B.BaseLine
				 LEFT JOIN ODLN T1 ON T0.DocEntry=T1.DocEntry
				 LEFT JOIN OITM I ON I.ItemCode=B.ItemCode
				 LEFT JOIN (
						SELECT * FROM (
							SELECT T0.BaseNum,T0.ItemCode,T0.BaseLinNum,T1.DistNumber As Batch,T1.LotNumber,Format(ExpDate,'dd-MMM-yyyy') AS ExpDate ,T0.Quantity
								-ISNULL((SELECT SUM(Quantity) FROM IBT1 C WHERE C.BsDocType=15 And C.BsDocEntry=T0.BaseEntry And C.BsDocLine=T0.BaseLinNum AND C.BatchNum=T0.BatchNum),0)
								-ISNULL((SELECT SUM(D.Quantity) FROM IBT1 D WHERE D.BaseType=14 AND D.BatchNum=T0.BatchNum And BaseEntry=(SELECT TOP 1 D1.TrgetEntry FROM INV1 D1 WHERE D1.BaseEntry=T0.BaseEntry And D1.BaseLine=T0.BaseLinNum)),0)
							 As Qty
							,T1.MnfSerial,T1.MnfDate,T0.BatchNum,Format(T1.CreateDate,'dd-MMM-yyyy') As InDate,T1.SysNumber
							FROM IBT1 T0
							LEFT JOIN OBTN T1 ON T0.ItemCode=T1.ItemCode And T0.BatchNum=T1.DistNumber
							LEFT JOIN OBTQ T2 ON T2.ItemCode=T1.ItemCode And T2.SysNumber=T1.SysNumber
							WHERE T0.BaseType IN(15) And T0.BaseNum=@NumberDO And T0.ItemCode=@par2 --AND T0.BaseNum=231000039 --T1.DistNumber IN('BN230623102027','BN230703110345')--T0.BaseType=16 AND 
							)A WHERE A.Qty<>0
				 )C ON C.BaseNum=T1.DocNum And B.ItemCode=C.ItemCode
				 LEFT JOIN (
					SELECT T0.BaseNum,T0.ItemCode,T0.BaseLinNum,T1.DistNumber As Batch,T1.LotNumber,Format(ExpDate,'dd-MMM-yyyy') AS ExpDate 
						,T0.Quantity -ISNULL((SELECT SUM(Quantity) FROM IBT1 C WHERE C.BaseType=14 AND C.BatchNum=T0.BatchNum And C.BaseEntry=(SELECT TOP 1 C1.TrgetEntry FROM INV1 C1 WHERE C1.DocEntry=T0.BaseEntry )),0) As Qty
						,T1.MnfSerial,T1.MnfDate,T1.SysNumber,Format(T1.CreateDate,'dd-MMM-yyyy') As InDate

					FROM IBT1 T0
						LEFT JOIN OBTN T1 ON T0.ItemCode=T1.ItemCode And T0.BatchNum=T1.DistNumber
						LEFT JOIN OBTQ T2 ON T2.ItemCode=T1.ItemCode AND T2.SysNumber=T1.SysNumber 
						WHERE T0.BaseType=13 And T0.BaseNum=@par1 And T0.ItemCode=@par2 --AND T2.Quantity=0
				 )D ON D.BaseNum=A.DocNum And B.ItemCode=D.ItemCode
			 WHERE A.CANCELED='N' And I.ManBtchNum='Y' And A.DocNum=@par1 And B.ItemCode=@par2
		) a
	END
	ELSE IF @TYPE='oDraft'
	BEGIN
		IF @par1='22' --update PO that has Save Goods Receipt PO to Draft
		BEGIN
			Declare @Close As INT
			Declare @POEntry AS INT
			SET @POEntry=(SELECT DocEntry FROM OPOR WHERE DocNum=Cast(@par2 As INT))

			UPDATE POR1 SET U_oDraftStatus=@par4 WHERE DocEntry=@POEntry And LineNum=@par5-- Update PO Line Status
			UPDATE POR1	SET U_ODraftQty=Cast(@par3 As Numeric) Where DocEntry=@POEntry And LineNum=@par5

			SET @Close =(SELECT Count (ItemCode) FROM POR1 Where DocEntry=@POEntry And ISNULL(U_oDraftStatus,'')<>'C')--Check status for update Document Header PO
			IF @Close>0
			BEGIN
				Update OPOR Set U_docDraft='' WHERE DocEntry=Cast(@POEntry As INT)
			END
			IF @Close=0
			BEGIN
				Update OPOR Set U_docDraft='oDraft' WHERE DocEntry=Cast(@POEntry As INT)
			END
			SELECT DocNum FROM OPOR WHERE DocNum=@par2
		END
		IF @par1='15'
		Begin
			Declare @Close16 As INT
			Declare @POEntry16 AS INT
			Declare @Qty1 As INT
			SET @POEntry16=(SELECT DocEntry FROM ODLN WHERE DocNum=Cast(@par2 As INT))

			SET @Qty1=(SELECT ISNULL(U_ODraftQty,0) FROM DLN1 WHere DocEntry=@POEntry16 And VisOrder=@par5)

			UPDATE DLN1 SET U_oDraftStatus=@par4 WHERE DocEntry=@POEntry16 And VisOrder=@par5-- Update PO Line Status
			UPDATE DLN1	SET U_ODraftQty=Cast(@par3 As Numeric)+Cast(@Qty1 As Numeric) Where DocEntry=@POEntry16 And VisOrder=@par5

			SET @Close16 =(SELECT Count (ItemCode) FROM DLN1 Where DocEntry=@POEntry16 And ISNULL(U_oDraftStatus,'')<>'C')--Check status for update Document Header PO
			IF @Close16>0
			BEGIN
				Update ODLN Set U_docDraft='' WHERE DocEntry=Cast(@POEntry16 As INT)
			END
			IF @Close16=0
			BEGIN
				Update ODLN Set U_docDraft='oDraft' WHERE DocEntry=Cast(@POEntry16 As INT)
			END
			SELECT DocNum FROM ODLN WHERE DocNum=@par2
		End
		IF @par1='13'--AR Invoice Save AR Memo Draft
		Begin
			Declare @Close13 As INT
			Declare @POEntry13 AS INT
			Declare @Qty13 As INT
			SET @POEntry13=(SELECT DocEntry FROM OINV WHERE DocNum=Cast(@par2 As INT))

			SET @Qty13=(SELECT ISNULL(U_ODraftQty,0) FROM INV1 WHere DocEntry=@POEntry13 And VisOrder=@par5)

			UPDATE INV1 SET U_oDraftStatus=@par4 WHERE DocEntry=@POEntry13 And VisOrder=@par5-- Update PO Line Status
			UPDATE INV1	SET U_ODraftQty=Cast(@par3 As Numeric)+Cast(@Qty13 As Numeric) Where DocEntry=@POEntry13 And VisOrder=@par5

			SET @Close13 =(SELECT Count (ItemCode) FROM INV1 Where DocEntry=@POEntry13 And ISNULL(U_oDraftStatus,'')<>'C')--Check status for update Document Header PO
			IF @Close13>0
			BEGIN
				Update OINV Set U_docDraft='' WHERE DocEntry=Cast(@POEntry13 As INT)
			END
			IF @Close13=0
			BEGIN
				Update OINV Set U_docDraft='oDraft' WHERE DocEntry=Cast(@POEntry13 As INT)
			END
			SELECT DocNum FROM OINV WHERE DocNum=@par2
		End
		IF @par1='SNB'--Serial Batch Return
		Begin
			Declare @CloseSN As INT
			Declare @POEntrySN AS INT
			Declare @OutSerial As Nvarchar(max)

			SET @POEntrySN=(SELECT DocEntry FROM ODLN WHERE DocNum=Cast(@par2 As INT))
			SET @OutSerial=(Select ISNULL(''+Cast(U_OutSerial As nvarchar(max)),'') From DLN1 Where DocEntry=@POEntrySN And VisOrder=@par3)
			UPDATE DLN1 SET U_OutSerial=IIF(@OutSerial='','',@OutSerial+',')+@par4 WHERE DocEntry=@POEntrySN And VisOrder=@par3-- Update PO Line Status
			SELECT 'Ok' As Ok
		End
		IF @par1='SNBAR'--Serial Batch Return
		Begin
			Declare @CloseSNAR As INT
			Declare @POEntrySNAR AS INT
			Declare @OutSerialAR As Nvarchar(max)

			SET @POEntrySNAR=(SELECT DocEntry FROM OINV WHERE DocNum=Cast(@par2 As INT))
			SET @OutSerialAR=(Select ISNULL(''+Cast(U_OutSerial As nvarchar(max)),'') From INV1 Where DocEntry=@POEntrySNAR And VisOrder=@par3)
			UPDATE INV1 SET U_OutSerial=IIF(@OutSerialAR='','',@OutSerialAR+',')+@par4 WHERE DocEntry=@POEntrySNAR And VisOrder=@par3-- Update PO Line Status
			SELECT 'Ok' As Ok
		End
	END
	ELSE IF @TYPE='GetItemBrand'
	Begin
		SELECT ItmsTypCod As Code,ItmsGrpNam As Name FROM OITG Order by Code
	End
	ELSE IF @TYPE='GetBarCode'
	BEGIN
		SELECT A.ItemCode,A.ItemName,C.ItmsGrpNam,A.CodeBars
		FROM OITM A 
		Left Join KPPT B ON A.ItemCode=B.ItemCode
		Left Join OITG C ON B.ItmsTypCod=C.ItmsTypCod
		WHERE ISNULL(A.CodeBars,'')<>'' And C.ItmsTypCod IN(Select A.Name From dbo.splitstring(@par1,',') A)
	END
	ELSE IF @TYPE='GetItemBarCode'
	Begin
		SELECT  A.ItemCode,A.ItemName,C.ItmsGrpNam,'*'+A.CodeBars+'*' As BarCode
		FROM OITM A 
		Left Join KPPT B ON A.ItemCode=B.ItemCode
		Left Join OITG C ON B.ItmsTypCod=C.ItmsTypCod
		WHERE ISNULL(A.CodeBars,'')<>'' And A.ItemCode IN(Select Name From dbo.splitstring(@par1,','))--A.ItemCode=@par1
	End
	ELSE IF @TYPE='GetItemInventoryTransferBranchToBranch'
	BEGIN
		select WhsCode into #tmp from OWHS WHERE CAST(BPLid AS nvarchar) = case when @par1='' Then BPLid Else @par1 End;
		select ItemCode,sum(OnHand) As Onhand into #tmp1 from OITW where OnHand<>0 AND WhsCode in (select WhsCode from #tmp) group by ItemCode
	SELECT 
			 A."ItemCode" AS ItemCode
			,ISNULL(A.ItemName,'' )AS ItemName
			,CAST(ISNULL(B.Price,0) AS float) AS CostingPrice
			,ISNULL(CAST(C.OnHand As int),0) As QtyinStock
			,CASE WHEN A.ManBtchNum='Y' THEN
				'B'
			 WHEN A.ManSerNum='Y' THEN
				'S'
			 ELSE
				'N'
			 END AS ItemType
			into #tmp2
		FROM OITM AS A 
		LEFT JOIN ITM1 AS B ON A.ItemCode=B.ItemCode AND B.PriceList='1'
		Right JOIN #tmp1 C ON A.ItemCode=C.ItemCode
		
		--LEFT JOIN OWHS C ON A1.ToWH = C.WhsCode
		--inner JOIN NNM1 D ON D.BPLId = C.BPLId
		WHERE A.validFor='Y' AND A.SellItem='Y' AND A.OnHand<>0  
		

		SELECT * 
			FROM #tmp2 WHERE QtyinStock<>0
			ORDER BY ItemCode DESC
			OFFSET cast(@par2 as int) ROWS
			FETCH NEXT cast(@par3 as int) ROWS ONLY;

		drop table #tmp
		drop table #tmp1
		drop table #tmp2
		
	END
	ELSE IF @TYPE='GetItemInventoryTransferBranchToBranch_Count'
	BEGIN
	
	select WhsCode into #tmp3 from OWHS WHERE CAST(BPLid AS nvarchar) = case when @par1='' Then BPLid Else @par1 End;
	select ItemCode,sum(OnHand) As Onhand into #tmp4 from OITW where OnHand<>0 AND WhsCode in (select WhsCode from #tmp3) group by ItemCode
	SELECT 
			 A."ItemCode" AS ItemCode
			,ISNULL(CAST(C.OnHand As int),0) As QtyinStock
			into #tmp5
		FROM OITM AS A 
		LEFT JOIN ITM1 AS B ON A.ItemCode=B.ItemCode AND B.PriceList='1'
		Right JOIN #tmp4 C ON A.ItemCode=C.ItemCode
		
		--LEFT JOIN OWHS C ON A1.ToWH = C.WhsCode
		--inner JOIN NNM1 D ON D.BPLId = C.BPLId
		WHERE A.validFor='Y' AND A.SellItem='Y' AND A.OnHand<>0  
		

		SELECT COUNT(ItemCode) AS 'Count' FROM #tmp5 WHERE QtyinStock<>0

		drop table #tmp3
		drop table #tmp4
		drop table #tmp5
		
		
	END
	ELSE IF @TYPE='GetSerailorBatchAvalableInventory'
		
		BEGIN
			
			IF @par1='S'
			BEGIN
					SELECT T0.ItemCode
					, T5.DistNumber As SerialOrBatch
					,CAST(T3.onHandQty As int) As Qty
					,CAST(T5.SysNumber AS nvarchar) AS SysNumber
					,Cast(Format(T5.InDate,'dd-MMM-yyyy') As Nvarchar(max)) As 'AdmissionDate',
					ISNULL(CONVERT(nvarchar(MAX),T4.ExpDate,111),'') AS ExpDate,
					ISNULL(T4.LotNumber,'') AS LotNumber
				FROM
					OIBQ T0
					inner join OBIN T1 on T0.BinAbs = T1.AbsEntry and T0.onHandQty <> 0
					left outer join OBBQ T2 on T0.BinAbs = T2.BinAbs and T0.ItemCode = T2.ItemCode and T2.onHandQty <> 0
					left outer join OSBQ T3 on T0.BinAbs = T3.BinAbs and T0.ItemCode = T3.ItemCode and T3.onHandQty <> 0
					left outer join OBTN T4 on T2.SnBMDAbs = T4.AbsEntry and T2.ItemCode = T4.ItemCode
					left outer join OSRN T5 on T3.SnBMDAbs = T5.AbsEntry and T3.ItemCode = T5.ItemCode
				WHERE
					T1.AbsEntry >= 0 
					and (T3.AbsEntry is not null)
					and T0.ItemCode in((select U0.ItemCode from OITM U0 inner join OITB U1 on U0.ItmsGrpCod = U1.ItmsGrpCod
					WHERE U0.ItemCode is not null 
					)) -- And T1.WhsCode in(Select A.Name From dbo.splitstring(@par3,',') A) 
					And T0.ItemCode IN(Select A0.Name From dbo.splitstring(@par2,',') A0)
			END
			ELSE IF @par1='B'
				BEGIN
				SELECT 
					A.ItemCode AS ItemCode,
					A.DistNumber AS SerialOrBatch,
					CAST(B.Quantity As int) AS Qty,
					CAST(A.SysNumber As nvarchar)AS SysNumber,
					ISNULL(A.LotNumber,'') AS LotNumber ,
					ISNULL(CAST(A.InDate As nvarchar),'')AS AdmissionDate,
					ISNULL(CAST(A.ExpDate As nvarchar),'') AS ExpDate,
					ISNULL(A.LotNumber,'') AS LotNumber
				FROM OBTN AS A
				LEFT JOIN OBTQ AS B ON A.ItemCode=B.ItemCode AND A.SysNumber=B.SysNumber
				WHERE A.ItemCode=@par2 AND B.Quantity!=0 --And B.WhsCode=@par3
			END
		END
	ELSE IF @TYPE='GetBranchAndDftWarehouse'
	BEGIN
		SELECT  BPLid AS BranchID
			   ,BPLName AS BranchName
			   ,DfltResWhs AS Warehouse
		FROM OBPL WHERE [Disabled]!='Y'
	END
	ELSE IF @TYPE='GetInventoryTransferBranchToBranchList'
	BEGIN
		SELECT 
			 A.DocEntry AS DocEntry
			,CAST(ISNULL(FORMAT(A.U_DocDate,'dd/MM/yyyy'),'') AS varchar) AS DocDate
			,ISNULL(A.U_CreateBy,'') AS CreateBy
			,ISNULL(A.U_Approve,'') AS Approve
			,ISNULL(B.BPLName, '') AS BranchFrom		
			,ISNULL(BB.BPLName, '') AS BranchTo
		FROM dbo.[@TB_OTRF_B_2_B] AS A
		LEFT JOIN OBPL AS B ON B.BPLId=A.U_BranchFrom
		LEFT JOIN OBPL AS BB ON BB.BPLId=A.U_BranchTo
		WHERE [Status]='O' AND A.U_Approve='Pending'
		ORDER BY DocEntry DESC
		OFFSET cast(@par3 as int) ROWS
		FETCH NEXT cast(@par4 as int) ROWS ONLY;
	END
	ELSE IF @TYPE='GetInventoryTransferBranchToBranchListCount'
	BEGIN
		SELECT 
			 COUNT(A.DocEntry) AS Count
		FROM dbo.[@TB_OTRF_B_2_B] AS A
		LEFT JOIN OBPL AS B ON B.BPLId=A.U_BranchFrom
		LEFT JOIN OBPL AS BB ON BB.BPLId=A.U_BranchTo
		WHERE [Status]='O' AND A.U_Approve='Pending'
	END
	ELSE IF @TYPE='GetInventoryTransferBranchToBranchHeaderByDocEntry'
	BEGIN
		SELECT 
			 A.DocEntry AS DocEntry
			,A.U_DocDate AS DocDate
			,A.U_Ref2 AS Ref2
			,B.BPLName AS BranchFrom		
			,BB.BPLName AS BranchTo
			,A.U_Remarks AS Remarks
		FROM dbo.[@TB_OTRF_B_2_B] AS A
		LEFT JOIN OBPL AS B ON B.BPLId=A.U_BranchFrom
		LEFT JOIN OBPL AS BB ON BB.BPLId=A.U_BranchTo
		WHERE [Status]='O' AND A.DocEntry=@par1;
	END
	ELSE IF @TYPE='GetInventoryTransferBranchToBranchLineByDocEntry'
	BEGIN
		SELECT 
			 A.U_ItemCode AS ItemCode
			,A.U_ItemName AS ItemName
			,CAST(A.U_Qty AS INT) AS Qty
			,CAST(A.U_Qty AS float) AS TotalCostingPrice
			,CAST(A.U_CostingPrice AS float) AS CostingPrice
			,A.U_ItemType AS ItemType
			,A.U_Serail_Batch AS Serail_Batch
		FROM dbo.[@TB_TRF1_B_2_B] AS A
		WHERE A.DocEntry=@par1;
	END
	ELSE IF @TYPE='GetLayoutInventoryDraft'
	BEGIN
		SELECT 
		ROW_NUMBER() OVER(ORDER BY ItemCode) AS No
			 ,A.U_ItemCode AS ItemCode
			,A.U_ItemCode AS ItemName
			,CAST(A.U_Qty AS INT) AS Qty
			,CAST(A.U_Qty AS float) AS TotalCostingPrice
			,CAST(A.U_CostingPrice AS float) AS CostingPrice
			,'50107-199' AS AccountCode
			,CASE WHEN B.ManBtchNum='Y' THEN 'B'
			 WHEN B.ManSerNum='Y' THEN 'S'
			 ELSE 'N' END AS ItemType
			,A.DocEntry AS DocEntry
			,A.U_Serail_Batch AS SerailBatch
			,B.InvntryUom AS UoMCode
			,FORMAT(A1.U_DocDate, 'MMM/dd/yyyy') AS DocDate
			,A1.U_BranchTo AS Branchto
			,A1.U_BranchFrom As BranchFrom
			,ISNULL(A1.U_Remarks,'') AS Remarks
		FROM dbo.[@TB_TRF1_B_2_B] AS A
		Left JOIN [@TB_OTRF_B_2_B] AS A1 ON A1.DocEntry=A.DocEntry
		LEFT JOIN OITM AS B ON A.U_ItemCode=B.ItemCode
		WHERE A.DocEntry=@par1
	END
	ELSE IF @TYPE='GetItemBOMs'
	BEGIN
		SELECT distinct A.Code,B.ItemName,B.FrgnName As 'ItemNameKH',cast(ISNULL(F.OnHand, 0) as float) OnHand, E.UomName Uom, ISNULL(A.ToWH, '') Warehouse
		FROM OITT A
			Inner JOIN OITM B ON A.Code=B.ItemCode
			LEFT JOIN OWHS C ON A.ToWH = C.WhsCode
			inner JOIN NNM1 D ON D.BPLId = C.BPLId
			LEFT JOIN OITW F ON B.ItemCode = F.ItemCode AND F.WhsCode = C.WhsCode
			LEFT JOIN OUOM E ON E.UomEntry = B.UgpEntry
		WHERE D.Series = @par2
		AND A.TreeType = 'P'
	END
	ELSE IF @TYPE='GetItemBOMInfo'
	BEGIN
		SELECT A.Code,B.ItemName, C.UomName Uom, A.ToWH Warehouse
		FROM OITT A
			LEFT JOIN OITM B ON A.Code=B.ItemCode
			LEFT JOIN OUOM C ON B.UgpEntry = C.UomEntry
		WHERE Code = @par1
	END
	ELSE IF @TYPE='GetListWarehouses'
	BEGIN
		SELECT WhsCode Code, OBPL.BPLName [BranchName] 
		FROM OWHS 
			LEFT JOIN OBPL ON OWHS.BPLid = OBPL.BPLId 
			LEFT JOIN NNM1 ON NNM1.BPLId = OBPL.BPLId
		WHERE NNM1.Series = @par1
		Order By WhsCode
	END
	ELSE IF @TYPE='GetListUsers'
	BEGIN
		SELECT USERID Id, ISNULL(U_NAME, USER_CODE) Name FROM OUSR Order By U_NAME
	END
	ELSE IF @TYPE='GetItemBOMsDetail'
	BEGIN
		SELECT 
		C.Type ItemType
		,C.Code ItemCode
		,IIF(C.Type<>4,E.ResName,D.ItemName) ItemName
		,cast(C.Quantity as float) BaseQty
		,cast(C.Quantity as float) PlannedQty
		,cast(0 as float) IssuedQty
		,cast((ISNULL(F.OnHand + F.OnOrder - F.IsCommited, 0)) as float) OnOrder
		,ISNULL(D.InvntryUom, '') InvntryUom
		,ISNULL(C.Warehouse, 'WH13') Warehouse
		,C.IssueMthd IssueType
		,cast(ISNULL(D.onHand,0) as float) OnHand
		,ISNULL(H.BPLName, '') Branch
		FROM OITT A
			LEFT JOIN OITM B ON A.Code=B.ItemCode
			LEFT JOIN ITT1 C ON C.Father=A.Code
			LEFT JOIN OITM D ON D.ItemCode=C.Code
			LEFT JOIN OITW F ON D.ItemCode = F.ItemCode AND F.WhsCode = A.ToWH
			LEFT JOIN(
				SELECT ResCode,ResName FROM ORSC
			) E ON E.ResCode=C.Code
			LEFT JOIN OWHS G ON G.WhsCode = C.Warehouse
			LEFT JOIN OBPL H ON H.BPLId = G.BPLid
		WHERE A.Code = @par1;
	END
	ELSE IF @TYPE='GetAllListGoodReceiptDraf'
	BEGIN
		SELECT 
			DocEntry As DocEntry
			,DocNum AS DocNum
			,Format( DocDate,'dd-MMM-yyyy' )AS PostingDate
			, CardCode AS VendorCode
			,CAST(DocTotal AS float)AS DocTotal

		FROM ODRF
		WHERE DocStatus='O' And U_docDraft<>'O' And ObjType='20'
		ORDER BY DocEntry DESC
		OFFSET cast(@par1 as int) ROWS
		FETCH NEXT cast(@par2 as int) ROWS ONLY;
	END
	ELSE IF @TYPE='GET_AllList_GoodReceiptDraft_Count'
	BEGIN
		SELECT 
			COUNT(DocNum) AS "Count" FROM ODRF 
			WHERE DocStatus='O' And U_docDraft<>'O' And ObjType='20'
	END
	ELSE IF @TYPE='GetHeaderGoodReceiptpoDrafbyDocEntry'
	BEGIN
		SELECT DISTINCT
			A.DocEntry AS DocEntry
			,A.CardCode AS CardCode
			,A.CardName AS CardName
			,Format(A.DocDate,'dd-MMM-yyyy' ) AS PostingDate
			,Format(A.DocDueDate,'dd-MMM-yyyy' )AS DueDate
			,A.DocNum  AS DocNum
			,B1.BPLName AS Branch
			,A.Comments AS Remarks
			,ISNULL(C.Name,'')AS ContactPerson
			,ISNULL(A.NumAtCard,'' )AS [Ref.No]
			,D.SeriesName AS SeriesName
		 FROM ODRF A
		 LEFT JOIN DRF1 AS B ON B.DocEntry=A.DocEntry
		 LEFT JOIN OBPL AS B1 ON B1.BPLId=A.BPLId
		 LEFT JOIN OCPR AS C ON C.CardCode=A.CardCode And C.CntctCode=A.CntctCode
		 LEFT JOIN NNM1 AS D On D.Series=A.Series and D.ObjectCode=20
		 Where A.DocStatus='O' and A.U_docDraft<>'O'  And A.ObjType='20' And A.DocEntry=@par1;
	END
	ELSE IF @TYPE='GetLineGoodreceiptpoDrafByDocEntry'
	BEGIN 
			
		SELECT 
			
			A.ItemCode AS ItemCode
			,A.Dscription AS ItemName
			,ISNULL(A.CodeBars,'' )AS BarCode
			,CAST(A.Quantity AS int)AS Quantity
			,CAST(A.Price AS float)AS UnitPrice
			,A.WhsCode AS WhsCode
			,A.VatGroup AS VAT
			,CAST(A.LineTotal AS float)AS LineTotal
			,A.UomCode AS UoM
			,CASE WHEN B.ManBtchNum='Y' THEN
						'B'
					 WHEN B.ManSerNum='Y' THEN
						'S'
					 ELSE
						'N'
					 END AS ManageItem
		FROM DRF1 A
		LEFT JOIN OITM AS B ON B.ItemCode=A.ItemCode
		Where  A.LineStatus='O' And A.ObjType='20' And A.DocEntry=@par1;
	END
	ELSE IF @TYPE='GetListGoodReturnDetail'
	BEGIN
		SELECT 
			DocEntry AS DocEntry
			,CardCode AS CardCode
			,CardName AS CardName
			,Format( DocDate,'dd-MMM-yyyy' )AS PostingDate
			,CAST(DocTotal AS float)AS DocTotal
		 FROM ODRF
		 where DocStatus='O' and ObjType='21'
		 ORDER BY DocEntry DESC
		OFFSET cast(@par1 as int) ROWS
		FETCH NEXT cast(@par2 as int) ROWS ONLY;
	END
	ELSE IF @TYPE='GetListGoodReturnDetail_Count'
	BEGIN
		SELECT 
			COUNT(DocEntry) AS "Count" FROM ODRF 
			WHERE DocStatus='O' and ObjType='21'
	END
	ELSE IF @TYPE='GetListGoodReturnAddDraft'
	BEGIN
		SELECT 
			DocEntry AS DocEntry
			,CardCode AS CardCode
			,CardName AS CardName
			,Format( DocDate,'dd-MMM-yyyy' )AS PostingDate
			,CAST(DocTotal AS float)AS DocTotal
		 FROM ODRF
		 where DocStatus='O' and ObjType='21' AND U_docDraft=@par1;
	END
	ELSE IF @TYPE='GetHeaderGoodsReturnByDocEntry'
	BEGIN 
		SELECT 
				A.DocEntry AS DocEntry
				,A.CardCode AS CardCode
				,A.CardName AS CardName
				,Format(A.DocDate,'dd-MMM-yyyy' ) AS PostingDate
				,Format(A.DocDueDate,'dd-MMM-yyyy' )AS DueDate
				,A.DocNum  AS DocNum
				,B1.BPLName AS Branch
				,A.Comments AS Remarks
				,ISNULL(C.Name,'')AS ContactPerson
				,ISNULL(A.NumAtCard,'' )AS RefNo
				,D.SeriesName AS SeriesName
		 FROM ODRF A
		 LEFT JOIN OBPL AS B1 ON B1.BPLId=A.BPLId
		 LEFT JOIN OCPR AS C ON C.CardCode=A.CardCode And C.CntctCode=A.CntctCode
		 LEFT JOIN NNM1 AS D On D.Series=A.Series and D.ObjectCode=21
		
		 where A.ObjType='21' and A.DocEntry=@par1;
	END
	ELSE IF @TYPE='GetLineItemGoodsReturnByDOcEntry'
	BEGIN

		SELECT 
				A.ItemCode AS ItemCode
				,A.Dscription AS ItemName
				,ISNULL(A.CodeBars,'') AS BarCode
				,CAST(A.Quantity AS int)AS QTY
				,CAST(A.Price AS float) AS UnitPrice
				,A.VatGroup AS VAT
				,CAST(A.LineTotal AS float) AS LineTotal
				,CASE WHEN B.ManBtchNum='Y' THEN
								'B'
							 WHEN B.ManSerNum='Y' THEN
								'S'
							 ELSE
								'N'
							 END AS ManageItem
				,CASE WHEN A.LineStatus='C' THEN
								'Complite'
							 WHEN A.LineStatus='O' THEN
								'Pandding'
							 END AS Type
		FROM DRF1 A
		LEFT JOIN OITM AS B ON B.ItemCode=A.ItemCode
		Where A.ObjType='21' AND A.DocEntry=@par1;
	END
	ELSE IF @TYPE='GetListDelivery'
	BEGIN
		SELECT 
				
			DocEntry AS DocEntry
			,CardCode AS VendorCode
			,CardName AS VendorName
			,CAST(DocTotal AS float)AS DocTotal 
			,Format(DocDate,'dd-MMM-yyyy') AS PostingDate
		FROM ODLN 
		WHERE  DocStatus='O'
		AND U_WebID<>'' And ObjType=15 And ISNULL(U_docDraft,'')<>'oDraft'
		 ORDER BY DocEntry DESC
		OFFSET cast(@par1 as int) ROWS
		FETCH NEXT cast(@par2 as int) ROWS ONLY;
	END
	ELSE IF @TYPE='GetListDelivery_Count'
	BEGIN
		SELECT 
			COUNT(DocEntry) AS "Count" FROM ODLN 
			WHERE  DocStatus='O'
			AND U_WebID<>'' And ObjType=15 And ISNULL(U_docDraft,'')<>'oDraft'
	END
	ELSE IF @TYPE='GetDeliveryHeaderByDocEntry'
	BEGIN
		SELECT
				A.DocEntry AS DocEntry
				,A.CardCode AS CardCode
				,A.CardName AS CardName
				,Format(A.DocDate,'dd-MMM-yyyy' ) AS PostingDate
				,Format(A.DocDueDate,'dd-MMM-yyyy' )AS DueDate
				,A.DocNum  AS DocNum
				,B1.BPLName AS Branch
				,ISNULL(A.Comments,'') AS Remarks
				,ISNULL(A.U_DeliCon,'' )AS DeliveryCondition
				,ISNULL(C.Name,'')AS ContactPerson
				,ISNULL(A.NumAtCard,'' )AS RefNo
				,D.SeriesName AS SeriesName
		 FROM ODLN A
		 LEFT JOIN OBPL AS B1 ON B1.BPLId=A.BPLId
		 LEFT JOIN OCPR AS C ON C.CardCode=A.CardCode And C.CntctCode=A.CntctCode
		 LEFT JOIN NNM1 AS D On D.Series=A.Series and D.ObjectCode=15

		 Where A.DocStatus='O' and A.ObjType='15' And U_WebID<>'' And ISNULL(U_docDraft,'')<>'oDraft' And A.DocEntry=@par1;
	END
	ELSE IF @TYPE='GetItemLineDeliveryDetailByDocEntry'
	BEGIN

			SELECT 
				A.ItemCode AS ItemCode
				,A.Dscription AS ItemName
				,ISNULL(A.CodeBars,'') AS BarCode
				,CAST(A.Quantity AS int)AS QTY
				,CAST(A.Price AS float) AS UnitPrice
				,A.VatGroup AS VAT
				,CAST(A.LineTotal AS float) AS LineTotal
				,CASE WHEN B.ManBtchNum='Y' THEN
								'B'
							 WHEN B.ManSerNum='Y' THEN
								'S'
							 ELSE
								'N'
							 END AS ManageItem
				,CASE WHEN A.LineStatus='O' THEN
								'Complite'
							 END AS Type
		FROM DLN1 A
		LEFT JOIN OITM AS B ON B.ItemCode=A.ItemCode
		Where A.ObjType='15' and A.LineStatus='O' And A.DocEntry=@par1;
	END
	ELSE IF @TYPE='GetallListofDeliveryReturn'
	BEGIN
		SELECT 

				DocEntry AS DocEntry
				,CardCode AS VendorCode
				,CardName AS VendorName
				,Format( DocDate,'dd-MMM-yyyy' )AS PostingDate
				,CAST(DocTotal AS float)AS DocTotal 

		FROM ODRF
				Where DocStatus='O' AND ObjType='16' And U_WebID<>'' And ISNULL(U_docDraft,'')<>'oDraft' 
				 ORDER BY DocEntry DESC
				OFFSET cast(@par1 as int) ROWS
				FETCH NEXT cast(@par2 as int) ROWS ONLY;

	END
	ELSE IF @TYPE='GetallListofDeliveryReturn_Count'
	BEGIN
		SELECT 
			COUNT(DocEntry) AS "Count" 
			FROM ODRF 
			Where DocStatus='O' AND ObjType='16' And U_WebID<>''
			 And ISNULL(U_docDraft,'')<>'oDraft' 
	END
	ELSE IF @TYPE='GetHeaderDeliveryReturnByDOnEntry'
	BEGIN
			SELECT
				A.DocEntry AS DocEntry
				,A.CardCode AS CardCode
				,A.CardName AS CardName
				,Format(A.DocDate,'dd-MMM-yyyy' ) AS PostingDate
				,Format(A.DocDueDate,'dd-MMM-yyyy' )AS DueDate
				,A.DocNum  AS DocNum
				,B1.BPLName AS Branch
				,ISNULL(A.Comments,'') AS Remarks
				,ISNULL(C.Name,'')AS ContactPerson
				,ISNULL(A.NumAtCard,'' )AS RefNo
				,D.SeriesName AS SeriesName
		 FROM ODRF A
		 LEFT JOIN OBPL AS B1 ON B1.BPLId=A.BPLId
		 LEFT JOIN OCPR AS C ON C.CardCode=A.CardCode And C.CntctCode=A.CntctCode
		 LEFT JOIN NNM1 AS D On D.Series=A.Series and D.ObjectCode=16

		 Where A.DocStatus='O' and A.ObjType='16' And U_WebID<>'' And A.DocEntry=@par1;
	END
	ELSE IF @TYPE='GetLineDeliveryReturnByDocEntry'
	BEGIN
				SELECT DISTINCT
					
				A.ItemCode AS ItemCode
				,A.Dscription AS ItemName
				,ISNULL(A.CodeBars,'') AS BarCode
				,CAST(A.Quantity AS int)AS QTY
				,CAST(A.Price AS float) AS UnitPrice
				,A.VatGroup AS VAT
				,CAST(A.LineTotal AS float) AS LineTotal
				,CASE WHEN B.ManBtchNum='Y' THEN
								'B'
							 WHEN B.ManSerNum='Y' THEN
								'S'
							 ELSE
								'N'
							 END AS ManageItem
				,CASE WHEN A.LineStatus='O' THEN
								'Complite'
							 END AS Type
							
		FROM DRF1 A
		LEFT JOIN OITM AS B ON B.ItemCode=A.ItemCode
		Where A.ObjType='16' and A.LineStatus='O'  And A.DocEntry=@par1;
	END
	ELSE IF @TYPE='GetAllListCraditMemo'
	BEGIN
		SELECT 

			DocEntry AS DocEntry
			,CardCode AS VendorCode
			,CardName AS VendorName
			,CAST(DocTotal AS float)AS DocTotal 
			,Format(DocDate,'dd-MMM-yyyy') AS PostingDate
		FROM ODRF

		Where ObjType=14 AND DocStatus='O' AND DocType='I'
		 ORDER BY DocEntry DESC
				OFFSET cast(@par1 as int) ROWS
				FETCH NEXT cast(@par2 as int) ROWS ONLY;
	END
	ELSE IF @TYPE='GetAllListCraditMemo_Count'
	BEGIN
		SELECT 
			COUNT(DocEntry) AS "Count" 
			FROM ODRF 
			Where ObjType=14 AND DocStatus='O' AND DocType='I'
	END
	ELSE IF @TYPE='GetHeaderCreditMemoByDocEntrys'
	BEGIN 
		SELECT 

				A.DocEntry AS DocEntry
				,A.CardCode AS CardCode
				,A.CardName AS CardName
				,Format(A.DocDate,'dd-MMM-yyyy' ) AS PostingDate
				,Format(A.DocDueDate,'dd-MMM-yyyy' )AS DueDate
				,A.DocNum  AS DocNum
				,B1.BPLName AS Branch
				,ISNULL(A.Comments,'') AS Remarks
				,ISNULL(A.U_DeliCon,'' )AS DeliveryCondition
				,ISNULL(C.Name,'')AS ContactPerson
				,ISNULL(A.NumAtCard,'' )AS RefNo
				,D.SeriesName AS SeriesName
		 FROM ODRF A
		 LEFT JOIN OBPL AS B1 ON B1.BPLId=A.BPLId
		 LEFT JOIN OCPR AS C ON C.CardCode=A.CardCode And C.CntctCode=A.CntctCode
		 LEFT JOIN NNM1 AS D On D.Series=A.Series and D.ObjectCode=14
		Where A.ObjType=14 AND A.DocStatus='O' AND A.DocType='I' AND A.DocEntry=@par1;
	END
	ELSE IF @TYPE='GetLineCreditMemoByDocEntrys'
	BEGIN
			SELECT 
				A.ItemCode AS ItemCode
				,A.Dscription AS ItemName
				,ISNULL(A.CodeBars,'') AS BarCode
				,CAST(A.Quantity AS int)AS QTY
				,CAST(A.Price AS float) AS UnitPrice
				,A.VatGroup AS VAT
				,CAST(A.LineTotal AS float) AS LineTotal
				,CASE WHEN B.ManBtchNum='Y' THEN
								'B'
							 WHEN B.ManSerNum='Y' THEN
								'S'
							 ELSE
								'N'
							 END AS ManageItem
				,CASE WHEN A.LineStatus='O' THEN
								'Complite'
							 END AS Type
		FROM DRF1 A
		LEFT JOIN OITM AS B ON B.ItemCode=A.ItemCode

		Where A.LineStatus='O' AND A.ObjType=14 AND A.DocEntry=@par1;
	END
	---------change store new--------------------

	ELSE IF @TYPE='GetUoM'
	BEGIN
		SELECT B.UomCode As UomGroup,C.UomCode
		FROM UGP1 A 
		LEFT JOIN OUOM B ON A.UgpEntry=B.UomEntry
		LEFT JOIN OUOM C ON A.UomEntry=C.UomEntry
	END
	ELSE IF @TYPE='GetNextNumber'
	BEGIN
		IF @par1='67'
		BEGIN
			SELECT NextNumber As DocNum FROM NNM1 WHERE ObjectCode=67 And Series=@par2
		END
		IF @par1='16'
		BEGIN
			SELECT NextNumber As DocNum FROM NNM1 WHERE ObjectCode=16 And Series=@par2
		END
		IF @par1='15'
		BEGIN
			SELECT NextNumber As DocNum FROM NNM1 WHERE ObjectCode=15 And Series=@par2
		END
		IF @par1='14'
		BEGIN
			SELECT NextNumber As DocNum FROM NNM1 WHERE ObjectCode=14 And Series=@par2
		END
		IF @par1='1470000065'
		BEGIN
			SELECT NextNumber As DocNum FROM NNM1 WHERE ObjectCode=1470000065 And Series=@par2
		END
		IF @par1='59'
		BEGIN
			SELECT NextNumber As DocNum FROM NNM1 WHERE ObjectCode=59 And Series=@par2
		END
	END
	ELSE IF @TYPE='OCPR'
	BEGIN
		SELECT CntctCode,Name FROM OCPR WHERE CardCode=@par1
	END
	ELSE IF @TYPE='Get_Availble_Serial'
	BEGIN
		SELECT T0.ItemCode, T5.DistNumber As [SerialBatch], Cast(T3.onHandQty As INT) As Qty
			,T1.WhsCode,T1.BinCode,Format(T5.ExpDate,'dd-MMM-yyyy') As 'ExpDate',T5.SysNumber
			,T4.LotNumber,Cast(Format(T5.MnfDate,'dd-MMM-yyyy') As Nvarchar(max)) As 'MnfDate' ,Cast(Format(T5.InDate,'dd-MMM-yyyy') As Nvarchar(max)) As 'AdmissionDate'
			,T5.MnfSerial As MfrNo
			-- T4.DistNumber, T4.MnfSerial,
			--T4.LotNumber, T5.MnfSerial, T5.LotNumber, T5.AbsEntry,
		    --T4.AbsEntry, T5.AbsEntry, T1.WhsCode,T0.BinAbs
		FROM
			OIBQ T0
			inner join OBIN T1 on T0.BinAbs = T1.AbsEntry and T0.onHandQty <> 0
			left outer join OBBQ T2 on T0.BinAbs = T2.BinAbs and T0.ItemCode = T2.ItemCode and T2.onHandQty <> 0
			left outer join OSBQ T3 on T0.BinAbs = T3.BinAbs and T0.ItemCode = T3.ItemCode and T3.onHandQty <> 0
			left outer join OBTN T4 on T2.SnBMDAbs = T4.AbsEntry and T2.ItemCode = T4.ItemCode
			left outer join OSRN T5 on T3.SnBMDAbs = T5.AbsEntry and T3.ItemCode = T5.ItemCode
		WHERE
			T1.AbsEntry >= 0 --and T1.WhsCode >= @WhsCode and T1.WhsCode <= @WhsCode 
			and (T3.AbsEntry is not null)
			and T0.ItemCode in((select U0.ItemCode from OITM U0 inner join OITB U1 on U0.ItmsGrpCod = U1.ItmsGrpCod
			WHERE U0.ItemCode is not null 
			))  And T1.WhsCode in(Select A.Name From dbo.splitstring(@par1,',') A) And T0.ItemCode IN(Select A0.Name From dbo.splitstring(@par2,',') A0)
		UNION ALL
		SELECT A.ItemCode,B.DistNumber,A.Quantity As Qty,A.WhsCode,'' As BinCode
		,B.ExpDate,A.SysNumber,B.LotNumber,B.MnfDate,B.InDate As AdmissionDate,B.MnfSerial
		
		FROM OSRQ A 
			LEFT JOIN OSRN B ON A.ItemCode=B.ItemCode And A.SysNumber=B.SysNumber And A.MdAbsEntry=B.AbsEntry
			LEFT JOIN OWHS C ON C.WhsCode=A.WhsCode 
		--	LEFT JOIN OBIN C ON B.AbsEntry=B.AbsEntry
		WHERE A.Quantity<>0 and A.ItemCode IN(Select A0.Name From dbo.splitstring(@par2,',') A0) And A.WhsCode IN (Select A.Name From dbo.splitstring(@par1,',') A) And C.BinActivat='N' --'ACCBINSTOB55'
		ORDER BY ItemCode
	END
	ELSE IF @TYPE='Get_Available_Batch'
	BEGIN
		SELECT ItemCode,BatchNum As SerialBatch,Quantity As Qty
		,WhsCode,'' As BinCode,Format(ExpDate,'dd-MMM-yyyy') As ExpDate ,SysNumber 
		,'' As 'LotNumber','' As 'MnfDate',Format(InDate,'dd-MMM-yyyy') As 'AdmissionDate'
		,'' As MfrNo
		FROM OIBT WHERE Direction=0 And Quantity<>0 
		And WhsCode IN(Select A.Name From dbo.splitstring(@par1,',')A) 
		And ItemCode IN(Select B.Name From dbo.splitstring(@par2,',')B)
	END
	ELSE IF @TYPE='TrfRequest_Status'
	BEGIN
		UPDATE OWTQ Set U_TrnRequestSts=@par1 Where DocNum=@par2
		SELECT 'Ok' As Ok
	END
	ELSE IF @TYPE='OWTR_OSRN'
	BEGIN
		SELECT T1.[ItemCode],T4.[IntrSerial] As SerialBatch, 1 As Qty ,T1.[WhsCode],'' As BinCode
			,Format(T4.ExpDate,'dd-MMM-yyyy') As ExpDate,T4.SysSerial As SysNumber,T5.LotNumber,Format(T5.MnfDate,'dd-MMM-yyyy') As MnfDate
			,Format(T4.InDate,'dd-MMM-yyyy') As AdmissionDate,T5.MnfSerial As MfrNo,T3.BaseLinNum As DocLine
		FROM OWTR T0 
			INNER JOIN WTR1 T1 ON T0.DocEntry = T1.DocEntry 
			INNER JOIN OITW T2 ON T1.ItemCode = T2.ItemCode AND T1.WhsCode = T2.WhsCode 
			LEFT JOIN SRI1 T3 ON T3.BaseEntry=T1.DocEntry AND T3.BaseLinNum=T1.LineNum AND T3.BaseType='67' 
			LEFT JOIN OSRI T4 ON T3.ItemCode=T4.ItemCode AND T3.SysSerial =T4.SysSerial 
			LEFT JOIN OSRN T5 ON T5.ItemCode=T4.ItemCode And T4.SysSerial=T5.SysNumber
		WHERE T3.[Direction] = '1' And 
		T0.DocNum=@par1
	END
	ELSE IF @TYPE='OWTR_OBTN'
	BEGIN
		SELECT  T0.ItemCode,T4.DistNumber As SerialBatch,T1.Quantity As Qty
		,T0.LocCode As WhsCode,'' As BinCode,T4.ExpDate,T4.SysNumber,T4.LotNumber,Format(T4.MnfDate,'dd-MMM-yyyy') As MnfDate
		,Format(T4.InDate,'dd-MMM-yyyy') As AdmissionDate,T4.MnfSerial As MfrNo,T0.DocLine
		FROM [OITL] T0
		INNER JOIN [ITL1] T1 ON T1.[LogEntry] = T0.[LogEntry]
		INNER JOIN OBTN T4 on T1.MdAbsEntry=T4.AbsEntry
		INNER JOIN [OITM] T2 ON T2.[ItemCode] = T0.[ItemCode]
		INNER JOIN OINM T5 on T0.ItemCode =T5.ItemCode and T0.LocCode = T5.Warehouse and T0.AppDocNum = T5.BASE_REF
		WHERE T0.DocNum=@par1 And T0.DocType=67 And T1.Quantity>0
	END
	ELSE IF @TYPE='GetOWTRHeader'
	BEGIN
		SELECT DocNum,Format(DocDate,'dd-MMM-yyyy') As DocDate,BPLId,BPLName,Filler As 'WhsFrom',ToWhsCode As 'WhsTo'
		FROM OWTR WHERE U_TrnRequestSts='C' And Series=Case When @par1='' Then Series Else @par1 End
	END
	ELSE IF @TYPE='GetWTR1'
	BEGIN
		SELECT --
			Cast(FORMAT(A.DocDate,'yyyy-MM-dd') As Nvarchar(max)) As DocDate,Cast(FORMAT(ShipDate,'yyyy-MM-dd') As Nvarchar(max)) As ShipDate			
			,ISNULL(A.CardCode,'') AS CardCode,ISNULL(A.CardName,'') AS CardName,ISNULL(A.CntctCode,'') AS ContactCode,ISNULL(A.[Address],'') AS [Address],ISNULL(A.JrnlMemo,'') As Remark
			,A.BPLId,A.BPLName,A.Filler As WhsFrom,A.ToWhsCode As WhsTo
			,A.DocEntry,B.LineNum,B.ItemCode,Dscription,Cast(Quantity As INT) As Qty
			,B.Price
			,B.LineTotal
			,UomCode,B.FromWhsCod As FromWhs,B.WhsCode As ToWhs
			--,ISNULL(C.ManBtchNum,'')+ISNULL(C.ManSerNum,'') As ManItem
			,Case When C.ManBtchNum='Y' Then 'Batch'
				  When C.ManSerNum='Y' Then 'Serial'
				  ELSE 'None' End As ItemType
			,C.CodeBars As BarCode
		FROM OWTR A
			LEFT JOIN WTR1 B ON A.DocEntry=B.DocEntry
			LEFT JOIN OITM C ON B.ItemCode=C.ItemCode
			LEFT JOIN OCPR D ON A.CntctCode=D.CntctCode
		WHERE A.U_TrnRequestSts='C' And A.DocNum IN(@par1)
	END
	ELSE IF @TYPE='GETBATCH_FOR_COUNTING'
	BEGIN
	SELECT
	 T0.ItemCode, T4.DistNumber As SerialBatch, T2.onHandQty As Qty, T1.WhsCode,
	T1.BinCode,Format(T4.ExpDate,'dd-MMM-yyyy') As ExpDate,T4.SysNumber,T4.LotNumber,T4.MnfDate,'' As MfrNo,Format(T4.InDate,'dd-MMM-yyyy') As AdmissionDate,T0.BinAbs
	FROM
	OIBQ T0
	inner join OBIN T1 on T0.BinAbs = T1.AbsEntry and T0.onHandQty <> 0
	left outer join OBBQ T2 on T0.BinAbs = T2.BinAbs and T0.ItemCode = T2.ItemCode and T2.onHandQty <> 0
	left outer join OSBQ T3 on T0.BinAbs = T3.BinAbs and T0.ItemCode = T3.ItemCode and T3.onHandQty <> 0
	left outer join OBTN T4 on T2.SnBMDAbs = T4.AbsEntry and T2.ItemCode = T4.ItemCode
	WHERE  T1.WhsCode IN(Select A.Name From dbo.splitstring(@par1,',')A) 
		And T0.ItemCode IN(Select B.Name From dbo.splitstring(@par2,',')B)
		And T4.DistNumber=@par3
		AND T0.BinAbs=@par4

	END
	ELSE IF @TYPE='GETSERIAL_FOR_COUNTING'
	BEGIN
		SELECT T0.ItemCode, T5.DistNumber As [SerialBatch], Cast(T3.onHandQty As INT) As Qty
			,T1.WhsCode,T1.BinCode,T1.AbsEntry,Format(T5.ExpDate,'dd-MMM-yyyy') As 'ExpDate',T5.SysNumber
			,T4.LotNumber,Cast(Format(T5.MnfDate,'dd-MMM-yyyy') As Nvarchar(max)) As 'MnfDate' 
			,Cast(Format(T5.InDate,'dd-MMM-yyyy') As Nvarchar(max)) As 'AdmissionDate'
			,T5.MnfSerial As MfrNo
			-- T4.DistNumber, T4.MnfSerial,
			--T4.LotNumber, T5.MnfSerial, T5.LotNumber, T5.AbsEntry,
		    --T4.AbsEntry, T5.AbsEntry, T1.WhsCode,T0.BinAbs
		FROM
			OIBQ T0
			inner join OBIN T1 on T0.BinAbs = T1.AbsEntry and T0.onHandQty <> 0
			left outer join OBBQ T2 on T0.BinAbs = T2.BinAbs and T0.ItemCode = T2.ItemCode and T2.onHandQty <> 0
			left outer join OSBQ T3 on T0.BinAbs = T3.BinAbs and T0.ItemCode = T3.ItemCode and T3.onHandQty <> 0
			left outer join OBTN T4 on T2.SnBMDAbs = T4.AbsEntry and T2.ItemCode = T4.ItemCode
			left outer join OSRN T5 on T3.SnBMDAbs = T5.AbsEntry and T3.ItemCode = T5.ItemCode
		WHERE
			T1.AbsEntry >= 0 --and T1.WhsCode >= @WhsCode and T1.WhsCode <= @WhsCode 
			and (T3.AbsEntry is not null)
			and T0.ItemCode in((select U0.ItemCode from OITM U0 inner join OITB U1 on U0.ItmsGrpCod = U1.ItmsGrpCod
			WHERE U0.ItemCode is not null 
			))  
			--And T1.WhsCode in(Select A.Name From dbo.splitstring(@par1,',') A) 
			--And T0.ItemCode IN(Select A0.Name From dbo.splitstring(@par2,',') A0)
			And T5.DistNumber=@par3
		
		ORDER BY T0.ItemCode
	END


	-------------Inventory Transfer thita-----------------------
	ELSE IF @TYPE='GET_BP_Inventory_Transfer'
	BEGIN
			SELECT	TOP 200
				ISNULL(A.CardCode,'') AS CardCode
				, ISNULL(A.CardName ,'') AS CardName
				,ISNULL(A0.Name,'') AS ContactPerson
				,ISNULL(ISNULL(A1.Address2+', ',''),'') + ISNULL(ISNULL(A1.Address3+', ',''),'') + ISNULL(A.City ,'') +  ISNULL(ISNULL(A.ZipCode+', ',''),'') + ISNULL(ISNULL(A1.Street+', ',''),'') + ISNULL(A2.Name,'') + ISNULL(A.Block,'') AS ShipTo
			FROM OCRD AS A
			LEFT JOIN CRD1 As A1 ON A1.CardCode=A.cardcode and A1.AdresType='S'
			LEFT JOIN OCPR AS A0 ON A0.CardCode=A.CardCode
			LEFT JOIN OCRY AS A2 ON A2.Code=A1.Country

			where A.CardType='C' AND A.validFor='Y';
			--WHERE A.CardCode=@par1;
	END
	ELSE IF @TYPE='GET_ITEM_Inventory_Transfer'
	BEGIN
		SELECT Top 500
			A."ItemCode" AS ItemCode
		,ISNULL(A.ItemName,'' )AS ItemName
		,CAST(A.OnHand AS float) As QtyinStock
		,CASE WHEN A.ManBtchNum='Y' THEN
			'B'
			WHEN A.ManSerNum='Y' THEN
			'S'
			ELSE
			'N'
			END AS ItemType
		,CAST(B.Price as float )AS UnitPrice
		,ISNULL(A.InvntryUom,'') AS UoM
		FROM OITM AS A 
		LEFT JOIN ITM1 AS B ON A.ItemCode=B.ItemCode AND B.PriceList='1'
		WHERE A.validFor='Y' AND A.SellItem='Y' AND A.OnHand<>0;
	END
	ELSE IF @TYPE='GET_ITEM_LINE_TRANSFER'
	BEGIN
		SELECT	top 100
				--A1.ItemCode
				--,A1.Dscription AS ItemName
				--,A1.Price
				--,A1.Quantity
				A.DocDate AS PostingDate
				,A.DocDueDate AS DocumentDate
				,A.BPLName AS FromBranch
				,A.ToWhsCode AS ToWarehouse
				,A.Filler AS FromWarehouse
				,A.DocNum
				,A0.SeriesName AS Series
				,ISNULL(A.Comments,'') AS Remarks
				,ISNULL(A.ToBinCode,'') As ToBinLocation
				,A1.UomCode AS UoM
		FROM OWTR AS A
		LEFT JOIN WTR1 AS A1 ON A1.DocEntry=A.DocEntry
		LEFT JOIN NNM1 AS A0 ON A0.Series=A.Series
		END
	ELSE IF @TYPE='GetBinLocation'
		BEGIN
			SELECT 
				A.WhsCode
				,A.BinCode
			 FROM OBIN AS A
			 Where A.WhsCode=@par1;
		END
	ELSE IF @TYPE='PriceList'
	BEGIN
		SELECT ListNum AS Code,ListName AS Name FROM OPLN
	END
	ELSE IF @TYPE='GetPermission'
	 BEGIN
		 SELECT * FROM [BarCodeDatabase].[dbo].Permission
	END
		ELSE IF @TYPE='FindPermission'
	BEGIN 
		SELECT A.Id FROM [BarCodeDatabase].[dbo].Permission A LEFT JOIN [BarCodeDatabase].[dbo].PermissionUser B ON B.PermissionId = A.Id WHERE B.UserCode = @Par1
	 END

END


--	EXEC [dbo].[USP_CALLTRANSCATION_POSKOFI] 'GetSerailorBatchAvalableInventory','S','ACCBINSTOB55','WH01','',''

--	EXEC [dbo].[USP_CALLTRANSCATION_POSKOFI] 'GetInventoryTransferBranchToBranchListCount','','','','',''
