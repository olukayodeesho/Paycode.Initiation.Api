USE [PaycodeDB]
GO
/****** Object:  Table [dbo].[CardlessWithdrawalTransaction]    Script Date: 04/13/2020 15:53:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CardlessWithdrawalTransaction](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TransactionReference] [varchar](50) NULL,
	[CIF] [varchar](50) NULL,
	[AccountNumber] [varchar](50) NULL,
	[RequestDate] [datetime] NULL,
	[ExpiryDate] [datetime] NULL,
	[TransactionAmount] [decimal](18, 2) NULL,
	[AmountAuthorized] [decimal](18, 2) NULL,
	[TokenUsageCount] [int] NULL,
	[SourceChannel] [varchar](50) NULL,
	[PaymentMethodTypeCode] [varchar](50) NULL,
	[PaymentMethodCode] [varchar](50) NULL,
	[FrontEndPartnerId] [varchar](50) NULL,
	[TokenLifeTimeInMinutes] [int] NULL,
	[PayWithMobileChannel] [varchar](50) NULL,
	[ProviderToken] [varchar](50) NULL,
	[TransactionType] [varchar](50) NULL,
	[CodeGenerationChannel] [varchar](50) NULL,
	[OneTimePassword] [varchar](50) NULL,
	[PayWithMobileToken] [varchar](50) NULL,
	[IsTokenUsed] [bit] NULL,
	[IsExpired] [bit] NULL,
	[IsCanceled] [bit] NULL,
	[AuthorizationSessionKey] [varchar](350) NULL,
	[ReversalSessionKey] [varchar](350) NULL,
	[LienID] [varchar](350) NULL,
 CONSTRAINT [PK_dbo.CardlessWithdrawalTransaction] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CardlessWithdrawalAuthorisationRequestLog]    Script Date: 04/13/2020 15:53:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CardlessWithdrawalAuthorisationRequestLog](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TransactionID] [varchar](50) NULL,
	[RequestDate] [datetime] NULL,
	[ISW_IP] [varchar](50) NULL,
	[Amount] [decimal](18, 2) NULL,
	[TransactionType] [varchar](50) NULL,
	[ProviderToken] [varchar](50) NULL,
	[AccountNumber] [varchar](50) NULL,
	[IsValid] [bit] NULL,
	[ResponseMessage] [varchar](100) NULL,
	[FinacleResponse] [varchar](150) NULL,
	[FinacleStan] [varchar](150) NULL,
	[FinacleTransactionDateTime] [varchar](150) NULL,
 CONSTRAINT [PK_dbo.CardlessWithdrawalAuthorisationRequestLog] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CardlessWithdrawalAuthorisation]    Script Date: 04/13/2020 15:53:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CardlessWithdrawalAuthorisation](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TransactionID] [varchar](50) NULL,
	[AuthorisationDate] [datetime] NULL,
	[ISW_IP] [varchar](50) NULL,
	[Amount] [decimal](18, 2) NULL,
	[TransactionType] [varchar](50) NULL,
	[AccountNumber] [varchar](50) NULL,
	[CardlessWithdrawalTransactionID] [int] NULL,
	[IsReversed] [bit] NULL,
	[DateReversed] [datetime] NULL,
	[FinacleResponse] [varchar](150) NULL,
	[FinacleStan] [varchar](150) NULL,
	[FinacleTransactionDateTime] [varchar](150) NULL,
 CONSTRAINT [PK_dbo.CardlessWithdrawalAuthorisation] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CardlessWithdrawalTransactionReversal]    Script Date: 04/13/2020 15:53:42 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CardlessWithdrawalTransactionReversal](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AuthorizationID] [int] NULL,
	[ReversalDate] [datetime] NULL,
	[ISW_IP] [varchar](50) NULL,
 CONSTRAINT [PK_dbo.CardlessWithdrawalTransactionReversal] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  ForeignKey [CardlessWithdrawalTransaction_CardlessWithdrawalAuthorisation]    Script Date: 04/13/2020 15:53:42 ******/
ALTER TABLE [dbo].[CardlessWithdrawalAuthorisation]  WITH CHECK ADD  CONSTRAINT [CardlessWithdrawalTransaction_CardlessWithdrawalAuthorisation] FOREIGN KEY([CardlessWithdrawalTransactionID])
REFERENCES [dbo].[CardlessWithdrawalTransaction] ([ID])
GO
ALTER TABLE [dbo].[CardlessWithdrawalAuthorisation] CHECK CONSTRAINT [CardlessWithdrawalTransaction_CardlessWithdrawalAuthorisation]
GO
/****** Object:  ForeignKey [CardlessWithdrawalAuthorisation_CardlessWithdrawalTransactionReversal]    Script Date: 04/13/2020 15:53:42 ******/
ALTER TABLE [dbo].[CardlessWithdrawalTransactionReversal]  WITH CHECK ADD  CONSTRAINT [CardlessWithdrawalAuthorisation_CardlessWithdrawalTransactionReversal] FOREIGN KEY([AuthorizationID])
REFERENCES [dbo].[CardlessWithdrawalAuthorisation] ([ID])
GO
ALTER TABLE [dbo].[CardlessWithdrawalTransactionReversal] CHECK CONSTRAINT [CardlessWithdrawalAuthorisation_CardlessWithdrawalTransactionReversal]
GO
