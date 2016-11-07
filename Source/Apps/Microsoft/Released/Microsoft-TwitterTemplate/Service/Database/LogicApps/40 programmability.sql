SET ANSI_NULLS              ON;
SET ANSI_PADDING            ON;
SET ANSI_WARNINGS           ON;
SET ANSI_NULL_DFLT_ON       ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER       ON;
go

CREATE PROCEDURE pbist_twitter.sp_get_replication_counts AS
BEGIN
    SET NOCOUNT ON;

    SELECT UPPER(LEFT(ta.name, 1)) + LOWER(SUBSTRING(ta.name, 2, 100)) AS EntityName, SUM(pa.[rows]) AS [Count]
    FROM sys.tables ta INNER JOIN sys.partitions pa ON pa.[OBJECT_ID] = ta.[OBJECT_ID]
                        INNER JOIN sys.schemas sc ON ta.[schema_id] = sc.[schema_id]
    WHERE
        sc.name='pbist_twitter' AND ta.is_ms_shipped = 0 AND pa.index_id IN (0,1) AND
        ta.name IN ('tweets_processed', 'tweets_normalized', 'hashtag_slicer', 'mention_slicer','authorhashtag_graph', 'authormention_graph')
    GROUP BY ta.name
END;
go


CREATE PROCEDURE pbist_twitter.sp_get_prior_content AS
BEGIN
    SET NOCOUNT ON;

    SELECT Count(*) AS ExistingObjectCount
    FROM   information_schema.tables
    WHERE  ( table_schema = 'pbist_twitter' AND
             table_name IN ('configuration', 'date', 'tweets_processed', 'tweets_normalized', 'hashtag_slicer', 'mention_slicer', 'entity_graph', 'authorhashtag_graph', 'authormention_graph', 'entities', 'entities2')
           );
END;
go

