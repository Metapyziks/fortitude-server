CSC = mcs

CSVERSION = future

SRC = -recurse:'*.cs'

DEF = LINUX

RESDIR = res
BINDIR = bin
LIBDIR = extlibs

DEBUGBINDIR = $(BINDIR)/debug
RELEASEBINDIR = $(BINDIR)/release

SYSLIBS = System.Data.dll,System.Core.dll,System.Web.dll
NINI = Nini.dll
SQLLIB = Mono.Data.Sqlite.dll

MAINCLASS = TestServer.Program

TARGET = TestServer.exe

release: 
	$(CSC) -langversion:$(CSVERSION) $(SRC) -r:$(SYSLIBS),$(LIBDIR)/$(NINI),$(SQLLIB) -d:$(DEF) \
		-main:$(MAINCLASS) -out:$(RELEASEBINDIR)/$(TARGET)
	cp -r $(LIBDIR) $(RELEASEBINDIR)
	cp -r $(RESDIR) $(RELEASEBINDIR)/$(RESDIR)
	cp config.ini $(RELEASEBINDIR)
	cp $(NINI) $(RELEASEBINDIR)

debug:
	$(CSC) -langversion:$(CSVERSION) $(SRC) -r:$(SYSLIBS),$(LIBDIR)/$(NINI),$(SQLLIB) -d:$(DEF),DEBUG \
		-main:$(MAINCLASS) -out:$(DEBUGBINDIR)/$(TARGET)
	cp -r $(LIBDIR) $(DEBUGBINDIR)
	cp -r $(RESDIR) $(DEBUGBINDIR)/$(RESDIR)
	cp config.ini $(DEBUGBINDIR)
	cp $(NINI) $(DEBUGBINDIR)

clean:
	rm TestServer.exe
