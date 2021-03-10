# This script uploads framework releases folder
# to the unsiave cloud storage.

s3cmd sync --skip-existing ./releases/ s3://unisave/unisave-framework/
