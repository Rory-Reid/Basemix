ALTER TABLE settings_profile ADD COLUMN
  litter_estimation_min_days_after_pairing INTEGER NOT NULL DEFAULT 21;

ALTER TABLE settings_profile ADD COLUMN
  litter_estimation_max_days_after_pairing INTEGER NOT NULL DEFAULT 23;

ALTER TABLE settings_profile ADD COLUMN
  litter_estimation_min_weaning INTEGER NOT NULL DEFAULT 25; -- 3 weeks 4 days

ALTER TABLE settings_profile ADD COLUMN
  litter_estimation_min_separation INTEGER NOT NULL DEFAULT 31; -- 4 weeks 3 days

ALTER TABLE settings_profile ADD COLUMN
  litter_estimation_min_rehome INTEGER NOT NULL DEFAULT 42; -- 6 Weeks
