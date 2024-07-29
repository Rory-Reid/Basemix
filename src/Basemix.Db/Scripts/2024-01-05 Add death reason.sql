CREATE TABLE IF NOT EXISTS death_reason
(
  id     INTEGER NOT NULL PRIMARY KEY,
  reason TEXT    NOT NULL
);

-- We need to let users check "dead" and then optionally enter death date and reason now
ALTER TABLE rat ADD COLUMN dead BOOLEAN NOT NULL DEFAULT FALSE;
ALTER TABLE rat ADD COLUMN death_reason_id INTEGER NULL REFERENCES death_reason(id);
ALTER TABLE rat DROP COLUMN death_reason; -- legacy idea, unused
UPDATE rat SET dead = TRUE where date_of_death IS NOT NULL;

INSERT INTO death_reason (reason)
VALUES ('Accident'),
       ('Tumor/Mass'),
       ('Tumor/Mass - Abdominal'),
       ('Tumor/Mass - Mammary'),
       ('Tumor/Mass - Zymbal''s Gland'),
       ('Tumor/Mass - Bladder'),
       ('Respiratory'),
       ('Respiratory - Chronic'),
       ('Respiratory - Acute'),
       ('Respiratory - Consolidated Lungs'),
       ('Abscess'),
       ('Abscess - Facial'),
       ('Pregnancy'),
       ('Birthing Difficulties'),
       ('Urinary Tract'),
       ('Urinary Tract - Infection'),
       ('Renal'),
       ('Renal - Kidney Failure'),
       ('Heart'),
       ('Heart - Heart Failure'),
       ('Neurological'),
       ('Neurological - Pituitary/Brain tumour'),
       ('Neurological - Stroke'),
       ('Infection'),
       ('Infection - Systemic'),
       ('Sudden Death'),
       ('Operation - Complications'),
       ('Operation - Post-op complications'),
       ('Malocclusion'),
       ('Virus'),
       ('Virus - Sendai'),
       ('Virus - SDAV')
