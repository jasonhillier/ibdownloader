FROM microsoft/dotnet:2.1-sdk

ADD . /ibdownloader
WORKDIR ibdownloader
RUN dotnet build -c release

WORKDIR /ibdownloader/IBDownloader/bin/Release/netcoreapp2.0

ENV IB_HOST=locahost
ENV IB_PORT=7982

ENTRYPOINT ["dotnet", "IBDownloader.dll"]
