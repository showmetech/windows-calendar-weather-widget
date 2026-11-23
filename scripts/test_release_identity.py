"""Regression: saved distribution metadata cannot restore an obsolete app name."""
import json
import os
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch
import xml.etree.ElementTree as ET
import release

class ReleaseIdentityTests(unittest.TestCase):
    def test_saved_legacy_display_name_is_normalized_without_changing_identity(self):
        original = {
            'identityName': 'Example.StableIdentity', 'publisher': 'CN=Example',
            'publisherDisplayName': 'Example Publisher', 'productId': 'EXAMPLE',
            'displayName': 'Windows Calendar e Weather Widget',
        }
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root/'store').mkdir()
            config_file = root/'store'/'identity.json'
            serialized = json.dumps(original)
            config_file.write_text(serialized, encoding='utf-8-sig')
            with patch.object(release, 'ROOT', root):
                config = release.identity()
            self.assertEqual(config['displayName'], 'SMT Calendar Widget')
            for key in ('identityName', 'publisher', 'publisherDisplayName', 'productId'):
                self.assertEqual(config[key], original[key])
            self.assertEqual(config_file.read_text(encoding='utf-8-sig'), serialized)
            for architecture in ('x64', 'arm64'):
                destination = root/(architecture+'.xml')
                release.manifest(config, architecture, destination)
                manifest = ET.parse(destination).getroot()
                ns = {'f': 'http://schemas.microsoft.com/appx/manifest/foundation/windows10'}
                identity = manifest.find('f:Identity', ns)
                self.assertEqual(identity.get('Name'), original['identityName'])
                self.assertEqual(identity.get('Publisher'), original['publisher'])
                self.assertEqual(identity.get('ProcessorArchitecture'), architecture)
                names = [element.text for element in manifest.iter() if element.tag.endswith('}DisplayName')]
                names += [element.get('DisplayName') for element in manifest.iter() if 'DisplayName' in element.attrib]
                self.assertEqual(names, ['SMT Calendar Widget']*3)
                startup = next(element for element in manifest.iter() if element.tag.endswith('}StartupTask'))
                self.assertEqual(startup.get('TaskId'), 'CalendarWeatherStartup')

    def test_environment_configuration_uses_versioned_name(self):
        with tempfile.TemporaryDirectory() as directory, patch.object(release, 'ROOT', Path(directory)), patch.dict(os.environ, {
            'STORE_IDENTITY_NAME': 'Example.StableIdentity', 'STORE_PUBLISHER': 'CN=Example',
            'STORE_PUBLISHER_DISPLAY_NAME': 'Example Publisher', 'STORE_PRODUCT_ID': 'EXAMPLE',
        }):
            self.assertEqual(release.identity()['displayName'], 'SMT Calendar Widget')

if __name__ == '__main__':
    unittest.main()
