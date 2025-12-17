import json
from typing import Dict, Any

class JSONManipulator:
    """
    JSONManipulator provides serialization and deserialization for dictionaries to/from JSON strings.
    """

    def serialize_dictionary(self, d: Dict[str, Any]) -> str:
        """Serialize a dictionary to a JSON string."""
        return json.dumps(d)

    def deserialize_dictionary(self, json_str: str) -> Dict[str, Any]:
        """Deserialize a JSON string to a dictionary."""
        return json.loads(json_str)
