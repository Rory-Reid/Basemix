CREATE TABLE IF NOT EXISTS breeder
(
    id      INTEGER NOT NULL PRIMARY KEY,
    name    TEXT    NULL,
    founded INTEGER NULL,
    active  BOOLEAN NOT NULL DEFAULT(0),
    owned   BOOLEAN NOT NULL DEFAULT(0)
);

CREATE TABLE IF NOT EXISTS rat
(
    id                INTEGER NOT NULL PRIMARY KEY,
    name              TEXT    NULL,
      
    sex               TEXT    NULL CHECK (sex IN ('Doe', 'Buck')),
    variety           TEXT    NULL,
    date_of_birth     INTEGER NULL,
    
    notes             TEXT    NULL,
    litter_id         INTEGER NULL,
    CONSTRAINT fk_litter FOREIGN KEY (litter_id) REFERENCES litter(id)
);

CREATE TABLE IF NOT EXISTS litter
(
    id              INTEGER NOT NULL PRIMARY KEY,
    dam_id          INTEGER NULL,
    sire_id         INTEGER NULL,
    date_of_birth   INTEGER NULL,
    date_of_pairing INTEGER NULL,
    CONSTRAINT fk_dam  FOREIGN KEY (dam_id)  REFERENCES rat(id),
    CONSTRAINT fk_sire FOREIGN KEY (sire_id) REFERENCES rat(id)
);

CREATE VIRTUAL TABLE IF NOT EXISTS rat_search USING fts5
(
    id,
    name,
    content=rat
);

CREATE TRIGGER IF NOT EXISTS rat_search_insert AFTER INSERT ON rat
BEGIN
    INSERT INTO rat_search (id, name) VALUES (new.id, new.name);
END;

CREATE TRIGGER IF NOT EXISTS rat_search_delete AFTER DELETE ON rat
BEGIN
    DELETE FROM rat_search WHERE id=old.id;
END;
