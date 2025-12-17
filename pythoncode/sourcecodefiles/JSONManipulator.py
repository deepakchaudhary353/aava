"""
JSONManipulator module converted from C# to Python.
Provides methods to serialize and deserialize dictionaries to/from JSON.
"""

import json
from typing import Dict, Any

class JSONManipulator:
    """
    A class for serializing and deserializing dictionaries using JSON.
    """

    def serialize_dictionary(self, d: Dict[str, Any]) -> str:
        """
        Serialize a dictionary to a JSON string.

        Args:
            d (Dict[str, Any]): The dictionary to serialize.

        Returns:
            str: The JSON string representation.
        """
        return json.dumps(d)

    def deserialize_dictionary(self, json_str: str) -> Dict[str, Any]:
        """
        Deserialize a JSON string to a dictionary.

        Args:
            json_str (str): The JSON string to deserialize.

        Returns:
            Dict[str, Any]: The resulting dictionary.
        """
        return json.loads(json_str)
