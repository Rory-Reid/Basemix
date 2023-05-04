CREATE TABLE IF NOT EXISTS owner
(
  id    INTEGER NOT NULL PRIMARY KEY,
  name  TEXT    NULL,
  email TEXT    NULL,
  phone TEXT    NULL,
  notes TEXT    NULL
);

ALTER TABLE rat ADD COLUMN owner_id INTEGER NULL REFERENCES owner(id);

CREATE VIRTUAL TABLE IF NOT EXISTS owner_search USING fts5
(
  id,
  name,
  content=owner
);

CREATE TRIGGER IF NOT EXISTS owner_search_insert AFTER INSERT ON owner
BEGIN
  INSERT INTO owner_search (id, name) VALUES (new.id, new.name);
END;

CREATE TRIGGER IF NOT EXISTS owner_search_delete AFTER DELETE ON owner
BEGIN
  DELETE FROM owner_search WHERE id=old.id;
END;
