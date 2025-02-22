use std::num::NonZero;

use eframe::egui::{self, ComboBox};

fn main() -> eframe::Result {
    let options = eframe::NativeOptions {
        viewport: egui::ViewportBuilder::default().with_inner_size([400.0, 400.0]),
        ..Default::default()
    };

    eframe::run_native(
        "Lab 2 | 13 Variant | Koshchei",
        options,
        Box::new(|cc| {
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

struct State {
    real_number: f32,
    real_numbers: Vec<f32>,

    numerator: i32,
    denominator: NonZero<u32>,
    fractions: Vec<(i32, NonZero<u32>)>,

    real_part: f32,
    imaginary_part: f32,
    complex_numbers: Vec<(f32, f32)>,

    most_occurring: Option<String>,
}

struct App {
    state: State,
    strategy: Box<dyn NumbersStrategy>,
}

impl Default for App {
    fn default() -> Self {
        Self {
            state: State {
                real_number: 0.0,
                real_numbers: vec![],

                numerator: 0,
                denominator: unsafe { NonZero::new_unchecked(1) },
                fractions: vec![],

                real_part: 0.0,
                imaginary_part: 0.0,
                complex_numbers: vec![],

                most_occurring: None,
            },
            strategy: Box::new(RealNumbersStrategy),
        }
    }
}

impl eframe::App for App {
    fn update(&mut self, ctx: &egui::Context, _frame: &mut eframe::Frame) {
        egui::CentralPanel::default().show(ctx, |ui| {
            ComboBox::from_label("Select Strategy")
                .selected_text(self.strategy.ui_name())
                .show_ui(ui, |ui| {
                    if ui
                        .selectable_label(true, RealNumbersStrategy.ui_name())
                        .clicked()
                    {
                        self.strategy = Box::new(RealNumbersStrategy);
                        self.state.most_occurring = None;
                    } else if ui
                        .selectable_label(false, FractionsStrategy.ui_name())
                        .clicked()
                    {
                        self.strategy = Box::new(FractionsStrategy);
                        self.state.most_occurring = None;
                    } else if ui
                        .selectable_label(false, ComplexNumbersStrategy.ui_name())
                        .clicked()
                    {
                        self.strategy = Box::new(ComplexNumbersStrategy);
                        self.state.most_occurring = None;
                    }
                });

            self.strategy.ui_input(ui, &mut self.state);

            ui.separator();

            let list = self.strategy.ui_list(&self.state);
            ui.horizontal_wrapped(|ui| {
                let remove_indexes: Vec<usize> = list
                    .iter()
                    .enumerate()
                    .filter_map(|(index, item)| ui.button(item).clicked().then_some(index))
                    .collect();

                for &index in remove_indexes.iter().rev() {
                    self.strategy.remove(&mut self.state, index);
                }
            });

            ui.separator();

            ui.horizontal_wrapped(|ui| {
                if ui.button("Find most occurring").clicked() {
                    self.state.most_occurring = Some(
                        self.strategy
                            .most_occurring(&self.state)
                            .unwrap_or("none".to_owned()),
                    );
                }

                if let Some(most_occurring) = &self.state.most_occurring {
                    ui.label(most_occurring);
                }
            })
        });
    }
}

/// Finds most occuring item in the Vector
fn most_occurring_item<T: PartialEq + Clone>(items: &[T]) -> Option<T> {
    if items.is_empty() {
        return None;
    }

    let mut counts: Vec<(T, usize)> = Vec::new();
    for item in items {
        match counts.iter_mut().find(|(key, _)| *key == *item) {
            Some((_, count)) => *count += 1,
            None => counts.push((item.clone(), 1)),
        }
    }

    counts
        .into_iter()
        .max_by_key(|&(_, count)| count)
        .map(|(item, _)| item)
}

trait NumbersStrategy {
    fn ui_name(&self) -> &'static str;

    fn ui_input(&self, ui: &mut egui::Ui, state: &mut State);
    fn ui_list(&self, state: &State) -> Vec<String>;

    fn remove(&self, state: &mut State, index: usize);

    fn most_occurring(&self, state: &State) -> Option<String>;
}

struct RealNumbersStrategy;
impl NumbersStrategy for RealNumbersStrategy {
    fn ui_input(&self, ui: &mut egui::Ui, state: &mut State) {
        ui.horizontal_wrapped(|ui| {
            ui.add(egui::DragValue::new(&mut state.real_number));

            if ui.button("Add").clicked() {
                state.real_numbers.push(state.real_number);
            }
        });
    }

    fn ui_list(&self, state: &State) -> Vec<String> {
        state.real_numbers.iter().map(|x| x.to_string()).collect()
    }

    fn remove(&self, state: &mut State, index: usize) {
        state.real_numbers.remove(index);
    }

    fn most_occurring(&self, state: &State) -> Option<String> {
        most_occurring_item(&state.real_numbers).map(|x| x.to_string())
    }

    fn ui_name(&self) -> &'static str {
        "Real Numbers"
    }
}

struct FractionsStrategy;
impl NumbersStrategy for FractionsStrategy {
    fn ui_input(&self, ui: &mut egui::Ui, state: &mut State) {
        ui.horizontal_wrapped(|ui| {
            ui.add(egui::DragValue::new(&mut state.numerator));
            ui.add(egui::DragValue::new(&mut state.denominator));

            if ui.button("Add").clicked() {
                state
                    .fractions
                    .push((state.numerator, state.denominator.try_into().unwrap()));
            }
        });
    }

    fn ui_list(&self, state: &State) -> Vec<String> {
        state
            .fractions
            .iter()
            .map(|x| format!("{}/{}", x.0, x.1))
            .collect()
    }

    fn remove(&self, state: &mut State, index: usize) {
        state.fractions.remove(index);
    }

    fn most_occurring(&self, state: &State) -> Option<String> {
        most_occurring_item(&state.fractions).map(|x| format!("{}/{}", x.0, x.1))
    }

    fn ui_name(&self) -> &'static str {
        "Fractions"
    }
}

struct ComplexNumbersStrategy;
impl NumbersStrategy for ComplexNumbersStrategy {
    fn ui_input(&self, ui: &mut egui::Ui, state: &mut State) {
        ui.horizontal_wrapped(|ui| {
            ui.add(egui::DragValue::new(&mut state.real_part));
            ui.add(egui::DragValue::new(&mut state.imaginary_part));

            if ui.button("Add").clicked() {
                state
                    .complex_numbers
                    .push((state.real_part, state.imaginary_part));
            }
        });
    }

    fn ui_list(&self, state: &State) -> Vec<String> {
        state
            .complex_numbers
            .iter()
            .map(|x| format!("{} + {}i", x.0, x.1))
            .collect()
    }

    fn remove(&self, state: &mut State, index: usize) {
        state.complex_numbers.remove(index);
    }

    fn most_occurring(&self, state: &State) -> Option<String> {
        most_occurring_item(&state.complex_numbers).map(|x| format!("{} + {}i", x.0, x.1))
    }

    fn ui_name(&self) -> &'static str {
        "Complex Numbers"
    }
}
