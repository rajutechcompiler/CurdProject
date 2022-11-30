UPDATE [SLRequestor] SET [Status] ='In Process', [DatePulled] = getdate(), [SLPullListsId] = @pullListId 
WHERE TableName=@tableName AND TableId=@tableId AND [Status] = @status
            