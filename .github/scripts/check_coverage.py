"""Fail CI when the merged Cobertura report is below 80% line coverage."""

import sys
from xml.etree import ElementTree


report = ElementTree.parse(sys.argv[1]).getroot()
if report.tag != "coverage":
    raise SystemExit("Expected a Cobertura coverage report.")

valid = int(report.attrib["lines-valid"])
covered = int(report.attrib["lines-covered"])
if valid <= 0 or not 0 <= covered <= valid:
    raise SystemExit("Invalid Cobertura line counts.")

print(f"Maintained-source line coverage: {covered}/{valid} ({covered / valid:.2%}).")
if covered * 5 < valid * 4:
    raise SystemExit("Maintained-source line coverage is below 80%.")
