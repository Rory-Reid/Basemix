CREATE TABLE IF NOT EXISTS media (
  id                TEXT PRIMARY KEY,
  image_format      TEXT NOT NULL,
  compression       TEXT NOT NULL,
  original_width    INTEGER NOT NULL,
  original_height   INTEGER NOT NULL,
  stored_width      INTEGER NOT NULL,
  stored_height     INTEGER NOT NULL,
  size_bytes        INTEGER NOT NULL,
  data              BLOB NOT NULL,
  original_filename TEXT NOT NULL,
  created_at        INTEGER NOT NULL
);
