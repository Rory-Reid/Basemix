CREATE TABLE IF NOT EXISTS breeder
(
    id      INTEGER NOT NULL PRIMARY KEY,
    name    TEXT    NOT NULL,
    founded INTEGER NULL,
    active  BOOLEAN NOT NULL DEFAULT(0),
    owned   BOOLEAN NOT NULL DEFAULT(0)
);

CREATE TABLE IF NOT EXISTS rat
(
    id                INTEGER NOT NULL PRIMARY KEY,
    name              TEXT    NOT NULL,
      
    sex               TEXT    NOT NULL CHECK (sex IN ('Doe', 'Buck')),
    date_of_birth     INTEGER NOT NULL,
  --   genotype          TEXT    NULL,
  --   variety           TEXT    NULL,
    
    notes             TEXT    NULL
  --   birth_notes       TEXT    NULL,
  --   birth_weight      INTEGER NULL,
  --   type_score                NULL     CHECK (type_score IN ('Excellent', 'Good', 'Average', 'Poor')),
  --   temperament_score         NULL     CHECK (temperament_score IN ('Excellent', 'Good', 'Average', 'Poor')),
    
  --   date_of_death     BIGINT  NULL,
  --   death_reason      TEXT    NULL,
    
  --   breeder_id        INTEGER NULL,
  --   CONSTRAINT fk_breeder FOREIGN KEY(breeder_id) REFERENCES breeder(id)
);

CREATE TABLE IF NOT EXISTS litter
(
    id            INTEGER NOT NULL PRIMARY KEY,
    dam_id        INTEGER NULL,
    sire_id       INTEGER NULL,
    date_of_birth INTEGER NULL,
    CONSTRAINT fk_dam  FOREIGN KEY (dam_id)  REFERENCES rat(id),
    CONSTRAINT fk_sire FOREIGN KEY (sire_id) REFERENCES rat(id)
);

CREATE TABLE IF NOT EXISTS litter_kin
(
    litter_id    INTEGER NOT NULL,
    offspring_id INTEGER NOT NULL,
    CONSTRAINT fk_litter    FOREIGN KEY (litter_id)    REFERENCES litter(id) ON DELETE CASCADE,
    CONSTRAINT fk_offspring FOREIGN KEY (offspring_id) REFERENCES rat(id) ON DELETE CASCADE,
    UNIQUE (litter_id, offspring_id)
)