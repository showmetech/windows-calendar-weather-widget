"""Reproducible Windows builds. No credentials or user data are included."""
import argparse, hashlib, json, os, pathlib, re, shutil, subprocess, sys, xml.etree.ElementTree as ET, zipfile
ROOT=pathlib.Path(__file__).resolve().parents[1]
PROJECT=ROOT/'src'/'WindowsCalendarWeatherWidget.csproj'
ARTIFACTS=ROOT/'artifacts'
STAGE=ROOT/'.artifacts'

def run(*args,timeout=None):
    subprocess.run([str(a) for a in args],cwd=ROOT,check=True,timeout=timeout)

def version():
    value=ET.parse(PROJECT).findtext('.//Version')
    if not re.fullmatch(r'\d+\.\d+\.\d+',value or ''): raise ValueError('Project Version must be major.minor.patch')
    return value

def sdk_tool(name):
    override=os.environ.get(name.upper().replace('.','_'))
    if override and pathlib.Path(override).is_file(): return override
    base=pathlib.Path(os.environ.get('ProgramFiles(x86)',r'C:\Program Files (x86)'))/'Windows Kits'/'10'/'bin'
    found=sorted(base.glob('*/x64/'+name),reverse=True)
    if not found:
        cache=pathlib.Path(os.environ.get('NUGET_PACKAGES',str(pathlib.Path.home()/'.nuget'/'packages')))
        found=sorted((cache/'microsoft.windows.sdk.buildtools'/'10.0.26100.4654'/'bin').glob('*/x64/'+name),reverse=True)
    if not found: raise RuntimeError('Install the official Windows SDK; missing '+name)
    return str(found[0])

def reject_private_files(folder):
    forbidden={'.dat','.pfx','.p12'}
    for path in folder.rglob('*'):
        if path.is_file() and (path.suffix.lower() in forbidden or 'client_secret' in path.name.lower() or path.name in ('settings.json','weather.json','accounts.json') or path.name.startswith('calendar-')):
            raise RuntimeError('Private file in distribution: '+str(path.relative_to(folder)))

def build():
    ARTIFACTS.mkdir(exist_ok=True); STAGE.mkdir(exist_ok=True)
    for arch in ('x64','arm64'):
        output=STAGE/arch
        if output.exists(): shutil.rmtree(output)
        run('dotnet','publish',PROJECT,'-c','Release','-r','win-'+arch,'--self-contained','true','-o',output,'--nologo')
        reject_private_files(output)
        if arch=='x64':
            tests=STAGE/'tests'
            run(output/'WindowsCalendarWeatherWidget.exe','--self-test',tests,'--offline',timeout=240)
            report=json.loads((tests/'verification.json').read_text(encoding='utf-8-sig'))
            if not report.get('passed'): raise RuntimeError('Regression tests failed; inspect '+str(tests/'verification.json'))
            shutil.copy2(tests/'verification.json',ARTIFACTS/'verification.json')
        shutil.make_archive(str(ARTIFACTS/f'Windows-Calendar-Weather-Widget-{version()}-{arch}'),'zip',output)
    write_hashes()

def identity():
    path=ROOT/'store'/'identity.json'
    value=json.loads(path.read_text(encoding='utf-8-sig')) if path.exists() else {
        'identityName':os.environ.get('STORE_IDENTITY_NAME',''), 'publisher':os.environ.get('STORE_PUBLISHER',''),
        'publisherDisplayName':os.environ.get('STORE_PUBLISHER_DISPLAY_NAME',''), 'productId':os.environ.get('STORE_PRODUCT_ID',''),
        'displayName':'Windows Calendar e Weather Widget'}
    missing=[key for key in ('identityName','publisher','publisherDisplayName','productId') if not value.get(key)]
    if missing: raise RuntimeError('Partner Center configuration missing: '+', '.join(missing))
    return value

def manifest(config,arch,destination):
    ns='http://schemas.microsoft.com/appx/manifest/foundation/windows10'
    uap='http://schemas.microsoft.com/appx/manifest/uap/windows10'
    uap5='http://schemas.microsoft.com/appx/manifest/uap/windows10/5'
    rescap='http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities'
    for prefix,url in (('',ns),('uap',uap),('uap5',uap5),('rescap',rescap)): ET.register_namespace(prefix,url)
    def add(parent,name,attributes=None,text=None):
        element=ET.SubElement(parent,'{'+ns+'}'+name,attributes or {})
        if text is not None: element.text=text
        return element
    package=ET.Element('{'+ns+'}Package',{'IgnorableNamespaces':'uap uap5 rescap'})
    add(package,'Identity',{'Name':config['identityName'],'Publisher':config['publisher'],'Version':version()+'.0','ProcessorArchitecture':arch})
    props=add(package,'Properties'); add(props,'DisplayName',text=config['displayName']);add(props,'PublisherDisplayName',text=config['publisherDisplayName']);add(props,'Logo',text='StoreAssets\\Logo50.png')
    deps=add(package,'Dependencies');add(deps,'TargetDeviceFamily',{'Name':'Windows.Desktop','MinVersion':'10.0.22000.0','MaxVersionTested':'10.0.26100.0'})
    resources=add(package,'Resources');add(resources,'Resource',{'Language':'pt-BR'})
    apps=add(package,'Applications');app=add(apps,'Application',{'Id':'CalendarWeather','Executable':'WindowsCalendarWeatherWidget.exe','EntryPoint':'Windows.FullTrustApplication'})
    ET.SubElement(app,'{'+uap+'}VisualElements',{'DisplayName':config['displayName'],'Description':'Previsão do tempo e calendários na área de trabalho.','Square150x150Logo':'StoreAssets\\Logo150.png','Square44x44Logo':'StoreAssets\\Logo44.png','BackgroundColor':'transparent'})
    extensions=add(app,'Extensions');extension=ET.SubElement(extensions,'{'+uap5+'}Extension',{'Category':'windows.startupTask','Executable':'WindowsCalendarWeatherWidget.exe','EntryPoint':'Windows.FullTrustApplication'})
    ET.SubElement(extension,'{'+uap5+'}StartupTask',{'TaskId':'CalendarWeatherStartup','Enabled':'true','DisplayName':config['displayName']})
    capabilities=add(package,'Capabilities');add(capabilities,'Capability',{'Name':'internetClient'});ET.SubElement(capabilities,'{'+rescap+'}Capability',{'Name':'runFullTrust'});add(capabilities,'DeviceCapability',{'Name':'location'})
    ET.ElementTree(package).write(destination,encoding='utf-8',xml_declaration=True)

def msix(test=False):
    config={'identityName':'Showmetech.CalendarWeather.PackagingTest','publisher':'CN=PackagingTest','publisherDisplayName':'Packaging Test','displayName':'TEST ONLY Calendar Weather','productId':'TEST-ONLY'} if test else identity()
    makeappx=sdk_tool('makeappx.exe'); destination=STAGE/'packaging-test' if test else ARTIFACTS
    destination.mkdir(parents=True,exist_ok=True)
    if test: (ARTIFACTS/'packaging-verification.json').unlink(missing_ok=True)
    bundle=destination/'packages';bundle.mkdir(exist_ok=True)
    for arch in ('x64','arm64'):
        source=STAGE/arch
        if not (source/'WindowsCalendarWeatherWidget.exe').exists(): raise RuntimeError('Run build first')
        if test:
            target=destination/arch
            shutil.copytree(source,target,dirs_exist_ok=True)
            source=target
        reject_private_files(source)
        run('dotnet','run','--project',ROOT/'tools'/'StoreAssets','-c','Release','--',ROOT/'src'/'Assets'/'Calendar-Fluent-preview.png',source/'StoreAssets')
        manifest(config,arch,source/'AppxManifest.xml')
        run(makeappx,'pack','/d',source,'/p',bundle/f'CalendarWeather-{arch}.msix','/o')
    package=destination/f'Windows-Calendar-Weather-Widget-{version()}.msixbundle'
    run(makeappx,'bundle','/d',bundle,'/p',package,'/o')
    with zipfile.ZipFile(package.with_suffix('.msixupload'),'w',zipfile.ZIP_DEFLATED) as upload:
        upload.write(package,package.name)
    if test:
        report={'passed':True,'identity':'TEST ONLY - not a Store identity','architectures':['x64','arm64'],'makeappxSemanticValidation':True,'signed':False,'installed':False,'storeCertified':False}
        with zipfile.ZipFile(package.with_suffix('.msixupload')) as upload:
            if upload.namelist()!=[package.name] or upload.testzip() is not None: raise RuntimeError('Invalid upload container')
        (ARTIFACTS/'packaging-verification.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
    else: write_hashes()

def write_hashes():
    files=sorted(p for p in ARTIFACTS.iterdir() if p.suffix in ('.zip','.msixbundle','.msix','.msixupload'))
    (ARTIFACTS/'SHA256.txt').write_text(''.join(hashlib.sha256(p.read_bytes()).hexdigest()+'  '+p.name+'\n' for p in files),encoding='utf-8')

def prepare_release():
    """Only one path to a release: commit -> remote CI -> Store submission -> local update."""
    if subprocess.check_output(['git','status','--porcelain'],cwd=ROOT,text=True).strip(): raise RuntimeError('Commit and review all changes first')
    run('gh','auth','status')
    tag='v'+version()
    run('git','push','origin','HEAD:main')
    existing=subprocess.check_output(['git','tag','--list',tag],cwd=ROOT,text=True).strip()
    if not existing: run('git','tag','-a',tag,'-m','Release '+tag)
    if subprocess.check_output(['git','rev-parse',tag+'^{}'],cwd=ROOT,text=True).strip()!=subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT,text=True).strip(): raise RuntimeError('Version tag belongs to another commit')
    run('git','push','origin',tag)
    print('Release requested. Do not install new desktop binaries until the release workflow and Store submission have succeeded. Store certification is a separate Microsoft decision.')

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('command',choices=('build','msix','msix-test','publish'));args=parser.parse_args()
    try: {'build':build,'msix':msix,'msix-test':lambda:msix(True),'publish':prepare_release}[args.command]()
    except Exception as error: print(str(error),file=sys.stderr);sys.exit(1)
