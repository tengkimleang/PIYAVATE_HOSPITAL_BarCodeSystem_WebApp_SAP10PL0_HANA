USE [KOFIDB_COLTD_16102023]
GO
/****** Object:  StoredProcedure [dbo].[CALL_TRANSCATION_POSKOFI_Raksmey]    Script Date: 2/16/2024 10:04:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER PROC [dbo].[CALL_TRANSCATION_POSKOFI_Raksmey]
	@TYPE AS NVARCHAR(MAX),
	@par1 AS NVARCHAR(MAX),
	@par2 AS NVARCHAR(MAX),
	@par3 AS NVARCHAR(MAX),
	@par4 AS NVARCHAR(MAX),
	@par5 AS NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;
	IF @TYPE='GetUoM'
	BEGIN
		SELECT B.UomCode As UomGroup,C.UomCode
		FROM UGP1 A 
		LEFT JOIN OUOM B ON A.UgpEntry=B.UomEntry
		LEFT JOIN OUOM C ON A.UomEntry=C.UomEntry
	END
	ELSE IF @par1='1250000001'--Inventory Transfer Request
		BEGIN
			SELECT B.Series,SeriesName,B.BPLId As 'Branch',(SELECT ISNULL(MAX(DocNum)+1,B.InitialNum) 
			FROM OWTQ WHERE Series=B.Series)AS DocNum FROM OFPR AS A 
				LEFT JOIN NNM1 AS B ON A.Indicator=B.Indicator
			WHERE Category=YEAR(GETDATE()) AND B.ObjectCode=1250000001 AND A.SubNum=MONTH(GETDATE())
		END
	ELSE IF @TYPE='SERIES'
	BEGIN
		IF @par1='67'--Inventory Transfer Request
		BEGIN
			SELECT A.Series,A.SeriesName,B.BPLId As 'Branch',B.BPLName,A.NextNumber As DocNum
			FROM NNM1 A 
				LEFT JOIN OBPL B ON A.BPLId=B.BPLId
			WHERE A.Indicator=Cast(YEAR(GETDATE()) As Nvarchar(max)) And A.ObjectCode=67
		END
		IF @par1='1470000065'
		BEGIN
			SELECT B.Series,SeriesName,B.BPLId As 'Branch',C.BPLName,(SELECT ISNULL(MAX(DocNum)+1,B.InitialNum) As DocNum	
			FROM OWTR WHERE Series=B.Series)AS DocNum FROM OFPR AS A 
				LEFT JOIN NNM1 AS B ON A.Indicator=B.Indicator
				LEFT JOIN OBPL AS C ON C.BPLId=B.BPLId
			WHERE B.Indicator=YEAR(GETDATE()) AND B.ObjectCode=1470000065 AND A.SubNum=MONTH(GETDATE()) And B.Indicator<>'Default'
		END
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
		FROM OWTQ 
		WHERE DocStatus='O' And Series=Case When @par1='' Then Series Else @par1 End --AND DocNum LIKE '%'+@par2+'%';
		--	And ISNULL(U_TrnRequestSts,'')<>'C'
	
	END
	ELSE IF @TYPE='GET_Inventory_Request_DetailByDocNum'
	BEGIN	
		SELECT --
			Cast(FORMAT(A.DocDate,'yyyy-MM-dd') As Nvarchar(max)) As DocDate,Cast(FORMAT(ShipDate,'yyyy-MM-dd') As Nvarchar(max)) As ShipDate			
			,ISNULL(A.CardCode,'') AS CardCode,ISNULL(A.CardName,'') AS CardName,ISNULL(A.NumAtCard,'') AS NumAtCard,ISNULL(A.[Address],'') AS [Address],ISNULL(A.JrnlMemo,'') As Remark
			,A.BPLId,A.BPLName,A.Filler As HWhsFrom,A.ToWhsCode As HWhsTo
			,A.DocEntry,B.VisOrder As LineNum,B.ItemCode,Dscription,Cast(Quantity As INT) As Qty
			,B.Price
			,B.LineTotal
			,UomCode,B.FromWhsCod,B.WhsCode
			--,ISNULL(C.ManBtchNum,'')+ISNULL(C.ManSerNum,'') As ManItem
			,Case When C.ManBtchNum='Y' Then 'Batch'
				  When C.ManSerNum='Y' Then 'Serial'
				  ELSE 'None' End As ManItem
			,A.BPLName As Branch
			,C.CodeBars As BarCode
			
		FROM OWTQ A
			LEFT JOIN WTQ1 B ON A.DocEntry=B.DocEntry
			LEFT JOIN OITM C ON B.ItemCode=C.ItemCode
		WHERE A.DocStatus='O' And A.DocNum IN(@par1) --LIKE '%'+@par2+'%';
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
	-------------------Inventory Transfer Return-------------
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
	ELSE IF @TYPE='GET_Inventory_StockCount'
	BEGIN
	
		SELECT 
			 DocNum AS 'DocNum'
			,A.CountDate AS 'CounterDate'
			,CASE WHEN A.CountType='1' THEN 'Single Counter' WHEN A.CountType='2' THEN 'Multiple Counters' END AS 'CounterType'
			,CASE WHEN LEN(A.[Time])=3 THEN LEFT(A.[Time],1)+':'+RIGHT(A.[Time],2) ELSE LEFT(A.[Time],2)+':'+RIGHT(A.[Time],2) END AS 'CountTime' 
			,IIF(A.CountType = 1, IIF(A.Taker1Type = 12, (SELECT U_NAME FROM OUSR WHERE USERID = A.Taker1Id) , (SELECt CONCAT(firstName, ' ', lastName) from OHEM WHERE empID = A.Taker1Id)), B.CounteName) AS CounteName
			INTO #TMP1

		FROM OINC AS A LEFT JOIN INC8 AS B ON A.DocEntry=B.DocEntry Where A.Status='O'--WHERE Series=@par1 And A.Status='O' --AND B.CounterId=@par3 AND DocNum LIKE '%'+@par2+'%';
		
		SELECT DISTINCT T1.DocNum,
						T1.CounterDate 
					   ,ISNULL(STUFF((SELECT DISTINCT ', ' + T2.CounteName 
									  FROM #TMP1 T2
									  WHERE T2.DocNum = T1.DocNum
									  FOR XML PATH('')), 1, 2, ''), '') AS [CounteName]
						,T1.CountTime

		FROM #TMP1 T1

		GROUP BY T1.DocNum, T1.CounterDate, T1.CountTime
		ORDER BY DocNum DESC
		OFFSET cast(@par4 as int) ROWS
		FETCH NEXT cast(@par5 as int) ROWS ONLY;
		drop table #TMP1
	
	END
	ELSE IF @TYPE='GET_Inventory_StockCount_Count'
	BEGIN
		SELECT 
			 DocNum AS 'DocNum'
			,A.CountDate AS 'CounterDate'
			,CASE WHEN A.CountType='1' THEN 'Single Counter' WHEN A.CountType='2' THEN 'Multiple Counters' END AS 'CounterType'
			,CASE WHEN LEN(A.[Time])=3 THEN LEFT(A.[Time],1)+':'+RIGHT(A.[Time],2) ELSE LEFT(A.[Time],2)+':'+RIGHT(A.[Time],2) END AS 'CountTime' 
			,IIF(A.CountType = 1, IIF(A.Taker1Type = 12, (SELECT U_NAME FROM OUSR WHERE USERID = A.Taker1Id) , (SELECt CONCAT(firstName, ' ', lastName) from OHEM WHERE empID = A.Taker1Id)), B.CounteName) AS CounteName
			INTO #TMP3

		FROM OINC AS A LEFT JOIN INC8 AS B ON A.DocEntry=B.DocEntry Where A.Status='O'--WHERE Series=@par1 And A.Status='O' --AND B.CounterId=@par3 AND DocNum LIKE '%'+@par2+'%';
		
		SELECT DISTINCT T1.DocNum,
						T1.CounterDate 
					   ,ISNULL(STUFF((SELECT DISTINCT ', ' + T2.CounteName 
									  FROM #TMP3 T2
									  WHERE T2.DocNum = T1.DocNum
									  FOR XML PATH('')), 1, 2, ''), '') AS [CounteName]
						,T1.CountTime
		INTO #TMP4
		FROM #TMP3 T1

		GROUP BY T1.DocNum, T1.CounterDate, T1.CountTime
		ORDER BY DocNum DESC

		SELECT COUNT(DocNum) AS COUNT FROM #TMP4 

		--drop table #TMP3
		drop table #TMP4
	END
	ELSE IF @TYPE='GET_Inventory_StockCount_Detail_ByDocNum'
	BEGIN
		SELECT 
			 A.CountDate AS CountDate
			,A.Series AS Series
			,A.DocNum AS DocNum
			,CASE WHEN A.CountType='1' THEN 'Single Counter' ELSE 'Multiple Counters' END AS CountType
			,Case When A.CountType='1' Then Cast(A.Taker1Id As nvarchar(max)) Else Cast(A.Taker1Id As nvarchar(max)) End As CountId
			,CASE WHEN LEN(A.[Time])=3 THEN LEFT(A.[Time],1)+':'+RIGHT(A.[Time],2) ELSE LEFT(A.[Time],2)+':'+RIGHT(A.[Time],2) END AS 'CountTime'
			,A.IndvCount AS IndvCount
			,A.TeamCount AS TeamCount
			--,C.CounterNum AS CounterNum
			--,C.CounteName AS CounterName
			--,ISNULL(D.CounterNum,0) AS TeamCounterNum
			--,ISNULL(D.CounteName,'') AS TeamCounterName
			,A.[Status] AS 'Status'
			,A.Ref2 AS Ref
			,Cast(A.Remarks As Nvarchar(max)) AS Remarks
			,B.ItemCode AS ItemCode
			,B.ItemDesc AS ItemName
			,ISNULL(B.InWhsQty,0) AS Qty
			,B.WhsCode AS WarehouseCode
			,CASE WHEN A.CountType='1' THEN B.CountQty ELSE F.TotalQty END AS QtyCounted
			,B.[Difference]
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
			,C.BinCode
			,B.BinEntry
			,B.UomCode
			,E.CodeBars
			,A.BPLId
			,G.UomCode As UomGroup
		FROM OINC AS A
		LEFT JOIN INC1 AS B ON A.DocEntry=B.DocEntry
		LEFT JOIN OBIN C ON C.AbsEntry=B.BinEntry
		LEFT JOIN OITM AS E ON E.ItemCode=B.ItemCode
		LEFT JOIN (
			SELECT DISTINCT INC9.DocEntry,INC8.CounterId,INC8.CounterNum,INC9.LineNum,TotalQty FROM INC9 LEFT JOIN INC8 ON INC8.CounterNum=INC9.CounterNum AND INC8.DocEntry=INC9.DocEntry
		)As F ON A.DocEntry=F.DocEntry AND F.LineNum=B.LineNum  
			AND F.CounterId = (SELECT TOP 1 A.CounterId FROM INC8 A LEFT JOIN OINC B ON A.DocEntry = B.DocEntry WHERE B.DocNum = @par1)  
			And F.CounterNum= (SELECT TOP 1 A.CounterNum FROM INC8 A LEFT JOIN OINC B ON A.DocEntry = B.DocEntry WHERE B.DocNum = @par1)  
		LEFT JOIN OUOM G ON G.UomEntry=E.UgpEntry
		WHERE  A.DocNum=@par1
	END
	ELSE IF @TYPE='Number_of_Individual_Counter'
	BEGIN
		SELECT CounterId AS CounterNum,CounteName AS CounterName FROM INC8 WHERE DocEntry= @par1
	END
	ELSE IF @TYPE='Number_of_Multiple_Counter'
	BEGIN
		SELECT ISNULL(CounterNum,0) AS TeamCounterNum,ISNULL(CounteName,'') AS TeamCounterName FROM INC4  WHERE DocEntry=@par1
	END
	--------------------Inventory Counting------------------
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
End

--	Exec [dbo].[CALL_TRANSCATION_POSKOFI_Raksmey] 'GET_Inventory_StockCount','231000013','21','','',''

