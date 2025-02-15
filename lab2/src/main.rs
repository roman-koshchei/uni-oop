use std::{
    collections::HashMap,
    num::{NonZero, NonZeroI32},
};

use eframe::egui::{self, ComboBox};

fn main() -> eframe::Result {
    let options = eframe::NativeOptions {
        viewport: egui::ViewportBuilder::default().with_inner_size([320.0, 240.0]),
        ..Default::default()
    };
    eframe::run_native(
        "Lab 2 | 13 Variant | Koshchei",
        options,
        Box::new(|cc| {
            // This gives us image support:
            // egui_extras::install_image_loaders(&cc.egui_ctx);

            use egui::FontFamily::{Monospace, Proportional};
            use egui::{FontId, TextStyle};
            cc.egui_ctx.style_mut(|style| {
                style.text_styles = [
                    (TextStyle::Heading, FontId::new(28.0, Proportional)),
                    (TextStyle::Body, FontId::new(20.0, Proportional)),
                    (TextStyle::Monospace, FontId::new(16.0, Monospace)),
                    (TextStyle::Button, FontId::new(20.0, Proportional)),
                    (TextStyle::Small, FontId::new(12.0, Proportional)),
                ]
                .into();
            });

            Ok(Box::<App>::default())
        }),
    )
}

struct App {
    number: i32,
    numbers: Vec<i32>,

    numerator: i32,
    denominator: NonZero<u32>,
    fractions: Vec<(i32, NonZero<u32>)>,

    strategy: Box<dyn NumbersStrategy>,
    most_occurring: Option<String>,
}

impl Default for App {
    fn default() -> Self {
        Self {
            number: 0,
            numbers: vec![],

            numerator: 1,
            denominator: NonZero::new(1).unwrap(),
            fractions: vec![],

            strategy: Box::new(RealNumbersStrategy),
            most_occurring: None,
        }
    }
}

impl eframe::App for App {
    fn update(&mut self, ctx: &egui::Context, _frame: &mut eframe::Frame) {
        egui::CentralPanel::default().show(ctx, |ui| {
            ComboBox::from_label("Select Strategy")
                .selected_text("Real Numbers")
                .show_ui(ui, |ui| {
                    if ui.selectable_label(true, "Real Numbers").clicked() {
                        self.strategy = Box::new(RealNumbersStrategy);
                    } else if ui.selectable_label(false, "Fractions").clicked() {
                        self.strategy = Box::new(FractionsStrategy);
                    } else if ui.selectable_label(false, "Complex Numbers").clicked() {
                        self.strategy = Box::new(ComplexNumbersStrategy);
                    }
                });

            ui.horizontal_wrapped(|ui| {
                // ui.add(egui::DragValue::new(&mut self.number));
                ui.add(egui::DragValue::new(&mut self.numerator));
                ui.add(egui::DragValue::new(&mut self.denominator));

                // if ui.button("Add").clicked() {
                //     self.numbers.push(self.number);
                // }
                if ui.button("Add").clicked() {
                    self.fractions
                        .push((self.numerator, self.denominator.try_into().unwrap()));
                }
            });

            let numbers: Vec<String> = self.strategy.numbers(&self);
            ui.separator();

            ui.horizontal_wrapped(|ui| {
                let mut remove_indexes = Vec::<usize>::new();

                for (i, number) in numbers.iter().enumerate() {
                    if ui.button(number.to_string()).clicked() {
                        remove_indexes.push(i);
                    }
                }

                for remove_index in remove_indexes {
                    self.fractions.remove(remove_index);
                }
            });

            ui.separator();

            ui.horizontal(|ui| {
                if ui.button("Find most occurring").clicked() {
                    self.most_occurring = Some(
                        self.strategy
                            .most_occurring(&self)
                            .unwrap_or("none".to_owned()),
                    );
                }

                if let Some(most_occurring) = &self.most_occurring {
                    ui.label(most_occurring);
                }
            })
        });
    }
}

trait NumbersStrategy {
    fn numbers(&self, app: &App) -> Vec<String>;
    fn most_occurring(&self, app: &App) -> Option<String>;
}

struct RealNumbersStrategy;
impl NumbersStrategy for RealNumbersStrategy {
    fn most_occurring(&self, app: &App) -> Option<String> {
        let mut frequency_map = HashMap::new();

        for &num in &app.numbers {
            *frequency_map.entry(num).or_insert(0) += 1;
        }

        frequency_map
            .into_iter()
            .max_by_key(|&(_, count)| count)
            .map(|(num, _)| num.to_string())
    }

    fn numbers(&self, app: &App) -> Vec<String> {
        app.numbers.iter().map(|num| num.to_string()).collect()
    }
}

struct FractionsStrategy;
impl NumbersStrategy for FractionsStrategy {
    fn most_occurring(&self, app: &App) -> Option<String> {
        todo!()
    }
    
    fn numbers(&self, app: &App) -> Vec<String> {
    app
                .fractions
                .iter()
                .map(|num| format!("{}/{}", num.0, num.1))
                .collect()
    }
}

struct ComplexNumbersStrategy;
impl NumbersStrategy for ComplexNumbersStrategy {
    fn most_occurring(&self, app: &App) -> Option<String> {
        todo!()
    }
    
    fn numbers(&self, app: &App) -> Vec<String> {
        todo!()
    }
}
