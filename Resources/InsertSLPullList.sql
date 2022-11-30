INSERT INTO [SLPullLists] ([OperatorsId], [DateCreated], [PriorityOrder], [BatchPullList], [BatchPrinted])
VALUES (@userName, getdate(), 2, @batchRequest, 0)
SELECT SCOPE_IDENTITY()
