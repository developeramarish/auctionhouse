$cs = "Data Source=127.0.0.1;Initial Catalog=AuctionhouseDatabase;TrustServerCertificate=True;User ID=sa;Password=Qwerty1234"
dotnet ef database update --configuration Test --connection "$cs" 04_Rm_QueuedConnections