"""Check distribution archives without reading any user's credential store."""
import json,pathlib,zipfile
ROOT=pathlib.Path(__file__).resolve().parents[1]
def audit():
    forbidden={'.dat','.pfx','.p12','.pdb'}
    private_names={'settings.json','accounts.json','weather.json'}
    checked=0
    archives=sorted((ROOT/'artifacts').glob('*.zip'))
    if not archives:raise RuntimeError('No release ZIPs to audit')
    for path in archives:
        with zipfile.ZipFile(path) as archive:
            if archive.testzip() is not None:raise RuntimeError('Corrupt archive: '+path.name)
            for name in archive.namelist():
                file=pathlib.PurePosixPath(name.replace('\\','/'))
                if file.suffix.lower() in forbidden or file.name in private_names or file.name.startswith(('calendar-','.widget-')) or 'client_secret' in file.name.lower():
                    raise RuntimeError('Private/test file in release: '+name)
                checked+=1
    report={'passed':True,'archives':[p.name for p in archives],'entriesChecked':checked,'scope':'Archive integrity and private-file names; not an exhaustive security certification'}
    (ROOT/'artifacts'/'privacy-file-audit.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
    print(json.dumps(report))
if __name__=='__main__':audit()
