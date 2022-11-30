SELECT COUNT(*) from [SLRequestor] WHERE (([SLRequestor].[Id] = @id) AND ([SLRequestor].[Status] = 'New'
AND (SLRequestor.SLPullListsId = 0 OR SLRequestor.SLPullListsId = IS NULL)
