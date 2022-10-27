CREATE TABLE IF NOT EXISTS breeder
(
  id     INTEGER NOT NULL PRIMARY KEY,
  name   TEXT    NOT NULL,
  active BOOLEAN NOT NULL DEFAULT(FALSE),
  owned  BOOLEAN NOT NULL DEFAULT(FALSE)
);

CREATE TABLE IF NOT EXISTS litter
(
  id         INTEGER NOT NULL PRIMARY KEY,
  birth_date INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS rat
(
  id                INTEGER NOT NULL PRIMARY KEY,
  name              TEXT    NOT NULL,
    
  sex               TEXT    NOT NULL CHECK (sex IN ('doe', 'buck')),
  date_of_birth     INTEGER NOT NULL,
  genotype          TEXT    NULL,
  variety           TEXT    NULL,
  
  notes             TEXT    NULL,
  birth_notes       TEXT    NULL,
  type_score                NULL     CHECK (type_score IN ('excellent', 'good', 'average', 'poor')),
  temperament_score         NULL     CHECK (temperament_score IN ('excellent', 'good', 'average', 'poor')),
  
  date_of_death     BIGINT  NULL,
  death_reason      TEXT    NULL,
  
  breeder_id        INTEGER NULL,
  litter_id         INTEGER NULL,
  constraint fk_breeder FOREIGN KEY(breeder_id) REFERENCES breeder(id),
  constraint fk_litter  FOREIGN KEY(litter_id)  REFERENCES litter(id)
);