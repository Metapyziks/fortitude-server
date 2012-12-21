CSC = mcs

CSVERSION = future

DEF = LINUX

SRCDIR = src
RESDIR = res
BINDIR = bin
LIBDIR = extlibs

SRC = \
	$(SRCDIR)/TestServer.AssemblyInfo.cs \
	$(SRCDIR)/Entities/*.cs \
	$(SRCDIR)/Requests/*.cs \
	$(SRCDIR)/Responses/*.cs \
	$(SRCDIR)/APIManager.cs \
	$(SRCDIR)/ActivationCode.cs \
	$(SRCDIR)/AuthSession.cs \
	$(SRCDIR)/ContentManager.cs \
	$(SRCDIR)/DatabaseManager.cs \
	$(SRCDIR)/EmailManager.cs \
	$(SRCDIR)/Program.cs \
	$(SRCDIR)/Serialization.cs \
	$(SRCDIR)/Tools.cs

DEBUGBINDIR = $(BINDIR)/debug
RELEASEBINDIR = $(BINDIR)/release

SYSLIBS = System.Data.dll,System.Core.dll,System.Web.dll
NINI = Nini.dll
SQLLIB = Mono.Data.Sqlite.dll

MAINCLASS = TestServer.Program

TARGET = TestServer.exe

release:
	mkdir -p $(RELEASEBINDIR)
	rm -f $(RELEASEBINDIR)/$(TARGET)
	$(CSC) -langversion:$(CSVERSION) $(SRC) -r:$(SYSLIBS),$(LIBDIR)/$(NINI),$(SQLLIB) -d:$(DEF) \
		-main:$(MAINCLASS) -out:$(RELEASEBINDIR)/$(TARGET)
	cp -r $(LIBDIR)/. $(RELEASEBINDIR)
	cp -r $(RESDIR) $(RELEASEBINDIR)/$(RESDIR)
	cp config.ini $(RELEASEBINDIR)

debug:
	mkdir -p $(DEBUGBINDIR)
	rm -f $(DEBUGBINDIR)/$(TARGET)
	$(CSC) -langversion:$(CSVERSION) $(SRC) -r:$(SYSLIBS),$(LIBDIR)/$(NINI),$(SQLLIB) -d:$(DEF),DEBUG \
		-main:$(MAINCLASS) -out:$(DEBUGBINDIR)/$(TARGET) -debug
	cp -r $(LIBDIR)/. $(DEBUGBINDIR)
	cp -r $(RESDIR) $(DEBUGBINDIR)/$(RESDIR)
	cp config.ini $(DEBUGBINDIR)

install:
	cp fortitude.init.sh /etc/init.d/fortitude
