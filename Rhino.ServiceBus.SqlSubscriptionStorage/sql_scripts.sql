CREATE SCHEMA RSB
GO

CREATE TABLE [RSB].[Subscription]
(
	[messageTypeName] [varchar](255) NOT NULL,
	[subscriberUri] [varchar](255) NOT NULL,
	CONSTRAINT [PK_Subscription_messageTypeName_subscriberUri] PRIMARY KEY CLUSTERED ([messageTypeName], [subscriberUri])
)
GO

CREATE PROCEDURE [RSB].[GetAllSubscriptionsForType]
(
	@messageTypeName VARCHAR(255)
)
AS 
BEGIN
	SELECT s2.[subscriberUri] 
	FROM [RSB].[Subscription] s2 WITH(NOLOCK)
	WHERE s2.[messageTypeName] = @messageTypeName
END
GO

CREATE PROCEDURE [RSB].[AddSubscription]
(
	@messageTypeName VARCHAR(255),
	@subscriberUri VARCHAR(255)
)
AS 
BEGIN
	IF NOT EXISTS (SELECT * FROM [RSB].[Subscription] s WITH(NOLOCK) WHERE s.[messageTypeName] = @messageTypeName AND s.[subscriberUri] = @subscriberUri)
	BEGIN
		INSERT INTO [RSB].[Subscription] (
			[messageTypeName],
			[subscriberUri]
		) VALUES ( 
			/* messageTypeName - varchar(255) */ @messageTypeName,
			/* subscriberUri - varchar(255) */ @subscriberUri )
		SELECT 1
	END
	ELSE 
	BEGIN
		SELECT 0
	END
END
GO

CREATE PROCEDURE [RSB].[RemoveSubscription]
(
	@messageTypeName VARCHAR(255),
	@subscriberUri VARCHAR(255)
)
AS 
BEGIN
	DELETE s 
	FROM [RSB].[Subscription] s WITH(ROWLOCK) 
	WHERE s.[messageTypeName] = @messageTypeName AND s.[subscriberUri] = @subscriberUri
END
GO