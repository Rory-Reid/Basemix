CREATE TABLE IF NOT EXISTS settings_profile
(
  -- Profile details
  id                               INTEGER NOT NULL PRIMARY KEY,
  name                             TEXT    NOT NULL,
  rattery_name                     TEXT    NULL,

  pedigree_footer                  TEXT    NULL,
  pedigree_show_sex                BOOLEAN NOT NULL DEFAULT 1,

  pedigree_pdf_page_margin         INTEGER NOT NULL DEFAULT 25,
  pedigree_pdf_font                TEXT    NOT NULL DEFAULT 'Carlito',
  pedigree_pdf_header_font_size    INTEGER NOT NULL DEFAULT 36,
  pedigree_pdf_subheader_font_size INTEGER NOT NULL DEFAULT 26,
  pedigree_pdf_font_size           INTEGER NOT NULL DEFAULT 10,
  pedigree_pdf_footer_font_size    INTEGER NOT NULL DEFAULT 10
);

INSERT INTO settings_profile (id, name) VALUES (1, 'Default');